using System.Diagnostics;
using System.Text;
using Deluxxe.IO;
using Deluxxe.RaceResults;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class ValidateDriversCliWorker(
    ActivitySource activitySource,
    ILogger<ValidateDriversCliWorker> logger,
    CompletionToken completionToken,
    RaffleRunConfiguration runConfiguration,
    RaceResultsService raceResultsService,
    StickerProviderUriResolver stickerProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var stickerManager = await stickerProvider.GetStickerManager(runConfiguration.stickerMapUri, runConfiguration.stickerMapSchemaVersion);
        var sponsorSums = new Dictionary<string, double>();
        var sponsorCounts = new Dictionary<string, int>();

        var prizeDescriptionRecords = await FileUriParser.ParseAndDeserializeSingleAsync<PrizeDescriptionRecords>(runConfiguration.prizeDescriptionUri, cancellationToken: token);

        foreach (var result in runConfiguration.raceResults)
        {
            var raceResults = await raceResultsService.GetAllDriversAsync(result.raceResultUri, runConfiguration.conditions, token);

            foreach (var raceResult in raceResults)
            {
                logger.LogInformation($"[driver={raceResult.name}][car={raceResult.carNumber}][owner{stickerManager.GetCandidateNameForCar(raceResult.carNumber, raceResult.name)}]");

                foreach (var prize in prizeDescriptionRecords.perRacePrizes)
                {
                    var status = stickerManager.DriverHasSticker(raceResult.carNumber, prize.name);
                    if (status is StickerStatus.StickerMapMissingForCar or StickerStatus.StickerValueMissingForCar)
                    {
                        logger.LogError($"unable determine sticker status for driver {raceResult.carNumber} with name {raceResult.name} for sponsor {prize.name}");
                    }
                }
            }
        }

        foreach (var raceResult in await raceResultsService.GetAllDriversAsync(runConfiguration.raceResults[0].raceResultUri, runConfiguration.conditions, token))
        {
            foreach (var sponsor in SponsorConstants.Sponsors)
            {
                var status = stickerManager.DriverHasSticker(raceResult.carNumber, sponsor);
                var val = status == StickerStatus.CarHasSticker ? 1.0 : 0.0;
                sponsorSums[sponsor] = sponsorSums.GetValueOrDefault(sponsor) + val;
                sponsorCounts[sponsor] = sponsorCounts.GetValueOrDefault(sponsor) + 1;
            }
        }

        const string sponsorFileName = "sponsor-representation.csv";
        if (File.Exists(sponsorFileName))
        {
            File.Delete(sponsorFileName);
        }

        await using var stream = new FileStream(sponsorFileName, FileMode.Create);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteLineAsync($"name,stat");
        foreach (var sponsor in SponsorConstants.Sponsors)
        {
            var stat = (sponsorSums[sponsor] / sponsorCounts[sponsor]) * 100;
            logger.LogInformation($"[sponsor={sponsor}][percentRepresented={stat}]");
            await writer.WriteLineAsync($"{sponsor},{stat}");
        }

        completionToken.Complete();
    }
}