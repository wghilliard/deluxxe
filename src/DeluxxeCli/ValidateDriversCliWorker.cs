using System.Diagnostics;
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

        var prizeDescriptionRecords = await FileUriParser.ParseAndDeserializeSingleAsync<PrizeDescriptionRecords>(runConfiguration.prizeDescriptionUri, cancellationToken: token);

        foreach (var result in runConfiguration.raceResults)
        {
            var raceResults = await raceResultsService.GetAllDriversAsync(result.raceResultUri, runConfiguration.conditions, token);

            foreach (var raceResult in raceResults)
            {
                logger.LogInformation($"mapped driver={raceResult.name} to car={raceResult.carNumber}, resolved owner as {stickerManager.GetCandidateNameForCar(raceResult.carNumber, raceResult.name)}");

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

        completionToken.Complete();
    }
}