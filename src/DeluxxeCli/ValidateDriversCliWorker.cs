using System.Diagnostics;
using System.Text;
using Deluxxe.IO;
using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class ValidateDriversCliWorker(
    ActivitySource activitySource,
    ILogger<ValidateDriversCliWorker> logger,
    CompletionToken completionToken,
    IDirectoryManager directoryManager,
    RaffleRunConfiguration runConfiguration,
    RaceResultsService raceResultsService,
    StickerProviderUriResolver stickerProvider,
    PreviousWinnerLoader previousWinnerLoader)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity(nameof(ValidateDriversCliWorker));

        var stickerManager = await stickerProvider.GetStickerManager(runConfiguration.stickerMapUri, runConfiguration.stickerMapSchemaVersion);

        var prizeDescriptionRecords = await FileUriParser.ParseAndDeserializeSingleAsync<PrizeDescriptionRecords>(runConfiguration.prizeDescriptionUri, directoryManager, cancellationToken: token);

        var prizeLimitChecker = new PrizeLimitChecker([..prizeDescriptionRecords.perEventPrizes, ..prizeDescriptionRecords.perRacePrizes]);
        var previousWinners = await previousWinnerLoader.LoadAsync(token);
        prizeLimitChecker.Update(previousWinners);

        foreach (var result in runConfiguration.raceResults)
        {
            var raceResults = await raceResultsService.GetAllDriversAsync(result.sessionId, runConfiguration.conditions, token);

            foreach (var raceResult in raceResults)
            {
                logger.LogInformation($"[driver={raceResult.name}][car={raceResult.carNumber}][owner={stickerManager.GetCandidateNameForCar(raceResult.carNumber, raceResult.name)}]");

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

        const string sponsorFileName = "sponsor-representation.csv";
        var sponsorFilePath = Path.Combine(directoryManager.collateralDir.FullName, sponsorFileName);
        if (File.Exists(sponsorFilePath))
        {
            File.Delete(sponsorFilePath);
        }

        var drivers = new List<Driver>();
        foreach (var result in runConfiguration.raceResults)
        {
            drivers.AddRange(await raceResultsService.GetAllDriversAsync(result.sessionId, runConfiguration.conditions, token));
        }

        drivers = drivers.Distinct().ToList();

        await using var stream = new FileStream(sponsorFilePath, FileMode.Create);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteLineAsync($"name,stat");
        foreach (var (sponsor, stat) in new RepresentationCalculator(stickerManager).Calculate(drivers))
        {
            logger.LogInformation($"[sponsor={sponsor}][percentRepresented={stat}]");
            await writer.WriteLineAsync($"{sponsor},{stat}");
        }


        logger.LogInformation("writing collateral pdf files");
        foreach (var session in runConfiguration.raceResults)
        {
            await raceResultsService.SaveResultsAsPdfAsync(session.sessionId, CancellationToken.None);
        }

        completionToken.Complete();
    }
}