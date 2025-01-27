using System.Diagnostics;
using Deluxxe.IO;
using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class RaffleCliWorker(
    ActivitySource activitySource,
    ILogger<RaffleCliWorker> logger,
    CompletionToken completionToken,
    RaffleRunConfiguration runConfiguration,
    RaffleService raffleService,
    RaceResultsService raceResultsService,
    IRaffleResultWriter raffleResultWriter,
    PreviousWinnerLoader previousWinnerLoader)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity("deluxxe-cli");
        var eventResourceIdBuilder = new ResourceIdBuilder().WithSeason(runConfiguration.season).WithEvent(runConfiguration.eventName, runConfiguration.eventId);

        // 2. get the prize descriptions
        // todo - validate the prize json
        var sponsorRecords = await FileUriParser.ParseAndDeserializeSingleAsync<SponsorRecords>(runConfiguration.prizeDescriptionUri, cancellationToken: token);
        var perRacePrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords!.perRacePrizes)
        {
            for (var count = 0; count < record.count; count++)
            {
                perRacePrizePrizeDescriptions.Add(new PrizeDescription()
                {
                    description = record.description,
                    sponsorName = record.name,
                    sku = record.sku
                });
            }
        }

        var prizeLimitChecker = new PrizeLimitChecker([..sponsorRecords.perEventPrizes, ..sponsorRecords.perRacePrizes]);

        var previousWinners = await previousWinnerLoader.LoadAsync(runConfiguration.previousResultsUri, token);
        prizeLimitChecker.Update(previousWinners);

        var eventRaceResults = new List<Driver>();
        var drawingResults = new List<DrawingResult>();
        var racePrizePreviousWinners = new List<PrizeWinner>();
        racePrizePreviousWinners.AddRange(previousWinners);
        foreach (var result in runConfiguration.raceResults)
        {
            // 4. get the race results
            var raceResults = await raceResultsService.GetAllDriversAsync(result.raceResultUri, runConfiguration.conditions, token);
            eventRaceResults.AddRange(raceResults);

            var raffleConfiguration = new RaffleConfiguration
            {
                MaxRounds = 5,
                DrawingType = DrawingType.Race,
                Season = runConfiguration.season,
                StickerMapUri = runConfiguration.stickerMapUri,
            };
            var drawingResult = await raffleService.ExecuteRaffleAsync(raffleConfiguration,
                perRacePrizePrizeDescriptions,
                raceResults,
                racePrizePreviousWinners,
                prizeLimitChecker,
                round => eventResourceIdBuilder.Copy().WithRaceDrawingRound(result.sessionName, result.sessionId, round.ToString()));
            drawingResults.Add(drawingResult);
            racePrizePreviousWinners.AddRange(drawingResult.winners);
            prizeLimitChecker.Update(drawingResult.winners);

            logger.LogInformation($"sat {drawingResult.winners.Count} won");
            logger.LogInformation($"sat {drawingResult.notAwarded.Count} not-awarded");
        }

        var perEventPrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords.perEventPrizes)
        {
            for (var count = 0; count < record.count; count++)
            {
                perEventPrizePrizeDescriptions.Add(new PrizeDescription()
                {
                    description = record.description,
                    sponsorName = record.name,
                    sku = record.sku
                });
            }
        }

        var eventRaffleConfig = new RaffleConfiguration
        {
            DrawingType = DrawingType.Event,
            MaxRounds = 5,
            Season = runConfiguration.season,
            StickerMapUri = runConfiguration.stickerMapUri
        };
        var eventDrawingResult = await raffleService.ExecuteRaffleAsync(eventRaffleConfig,
            perEventPrizePrizeDescriptions,
            eventRaceResults,
            previousWinners,
            prizeLimitChecker,
            round => eventResourceIdBuilder.Copy().WithEventDrawingRound(round.ToString()));
        drawingResults.Add(eventDrawingResult);
        logger.LogInformation($"event {eventDrawingResult.winners.Count} won");
        logger.LogInformation($"event {eventDrawingResult.notAwarded.Count} not-awarded");

        var raffleResult = new RaffleResult
        {
            drawings = drawingResults,
            resourceId = eventResourceIdBuilder.Build(),
            name = ResourceIdBuilder.NormalizeEventName(runConfiguration.eventName),
            season = runConfiguration.season,
            configurationName = runConfiguration.name
        };

        await raffleResultWriter.WriteAsync(raffleResult, token);

        completionToken.Complete();
    }
}