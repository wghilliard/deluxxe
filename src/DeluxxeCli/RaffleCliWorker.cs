using System.Diagnostics;
using System.Text.Json;
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
    IEnumerable<IRaffleResultWriter> raffleResultWriters,
    PreviousWinnerLoader previousWinnerLoader,
    IDirectoryManager directoryManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity(nameof(RaffleCliWorker));
        activity?.AddTag("raffleConfiguration", JsonSerializer.Serialize(runConfiguration.raffleConfiguration));

        var eventResourceIdBuilder = new ResourceIdBuilder().WithSeason(runConfiguration.season).WithEvent(runConfiguration.eventName, runConfiguration.eventId);

        // 2. get the prize descriptions
        var prizeDescriptionRecords = await FileUriParser.ParseAndDeserializeSingleAsync<PrizeDescriptionRecords>(runConfiguration.prizeDescriptionUri, directoryManager, cancellationToken: token);

        if (prizeDescriptionRecords.perRacePrizes.Count == 0 && prizeDescriptionRecords.perEventPrizes.Count == 0)
        {
            logger.LogError("unable to load sponsor records from URI");
        }

        var exceptions = PrizeDescriptionRecordValidator.Validate(prizeDescriptionRecords);

        foreach (var exception in exceptions)
        {
            logger.LogError(exception.ToString());
            return;
        }

        logger.LogInformation("sponsor records validated");

        var prizeLimitChecker = new PrizeLimitChecker([..prizeDescriptionRecords.perEventPrizes, ..prizeDescriptionRecords.perRacePrizes]);

        var previousWinners = await previousWinnerLoader.LoadAsync(token);
        prizeLimitChecker.Update(previousWinners);

        var eventRaceResults = new List<Driver>();
        var drawingResults = new List<DrawingResult>();
        var racePrizePreviousWinners = new List<PrizeWinner>();
        if (runConfiguration.raffleConfiguration.filterDriversWithWinningHistory)
        {
            racePrizePreviousWinners.AddRange(previousWinners);
        }

        foreach (var result in runConfiguration.raceResults)
        {
            var raceDrawingActivity = activitySource.StartActivity("race-drawing");

            // 4. get the race results
            var eligibleDrivers = await raceResultsService.GetAllDriversAsync(result.sessionId, runConfiguration.conditions, token);
            var perRacePrizePrizeDescriptions = PrizeDescriptionGenerator.GeneratePrizeDescriptions(prizeDescriptionRecords.perRacePrizes, eligibleDrivers);

            eventRaceResults.AddRange(eligibleDrivers);

            var raffleConfiguration = new RaffleExecutionConfiguration
            {
                MaxRounds = runConfiguration.raffleConfiguration.maxRounds,
                ClearHistoryIfNoCandidates = runConfiguration.raffleConfiguration.clearHistoryIfNoCandidates,
                DrawingType = DrawingType.Race,
                Season = runConfiguration.season,
                StickerMapUri = runConfiguration.stickerMapUri,
                StickerMapSchemaVersion = runConfiguration.stickerMapSchemaVersion,
                RandomShuffleSeed = runConfiguration.raffleConfiguration.randomShuffleSeed,
                RandomDrawingSeed = runConfiguration.raffleConfiguration.randomDrawingSeed,
                LimitOnePrizePerDriverPerWeekend = runConfiguration.raffleConfiguration.limitOnePrizePerDriverPerWeekend,
                SessionStartTime = result.startTime ?? DateTimeOffset.UtcNow.ToString()
            };
            var drawingResult = await raffleService.ExecuteRaffleAsync(raffleConfiguration,
                perRacePrizePrizeDescriptions,
                eligibleDrivers,
                racePrizePreviousWinners,
                prizeLimitChecker,
                round => eventResourceIdBuilder.Copy().WithRaceDrawingRound(result.sessionName, result.sessionId, round.ToString()));

            drawingResults.Add(drawingResult);
            if (runConfiguration.raffleConfiguration.limitOnePrizePerDriverPerWeekend)
            {
                racePrizePreviousWinners.AddRange(drawingResult.winners);
            }

            prizeLimitChecker.Update(drawingResult.winners);

            logger.LogInformation($"{drawingResult.winners.Count} won");
            logger.LogInformation($"{drawingResult.notAwarded.Count} not-awarded");
            raceDrawingActivity?.Dispose();
        }

        var perEventPrizePrizeDescriptions = PrizeDescriptionGenerator.GeneratePrizeDescriptions(prizeDescriptionRecords.perEventPrizes, eventRaceResults);

        var eventDrawingActivity = activitySource.StartActivity("event-drawing");

        var eventPrizePreviousWinners = new List<PrizeWinner>();
        if (runConfiguration.raffleConfiguration.filterDriversWithWinningHistory)
        {
            eventPrizePreviousWinners.AddRange(previousWinners);
        }

        if (runConfiguration.raffleConfiguration.limitOnePrizePerDriverPerWeekend)
        {
            eventPrizePreviousWinners.AddRange(racePrizePreviousWinners);
        }

        var eventRaffleConfig = new RaffleExecutionConfiguration
        {
            MaxRounds = runConfiguration.raffleConfiguration.maxRounds,
            ClearHistoryIfNoCandidates = runConfiguration.raffleConfiguration.clearHistoryIfNoCandidates,
            DrawingType = DrawingType.Event,
            Season = runConfiguration.season,
            StickerMapUri = runConfiguration.stickerMapUri,
            StickerMapSchemaVersion = runConfiguration.stickerMapSchemaVersion,
            RandomShuffleSeed = runConfiguration.raffleConfiguration.randomShuffleSeed,
            RandomDrawingSeed = runConfiguration.raffleConfiguration.randomDrawingSeed,
            LimitOnePrizePerDriverPerWeekend = runConfiguration.raffleConfiguration.limitOnePrizePerDriverPerWeekend,
            SessionStartTime = DateTimeOffset.UtcNow.ToString()
        };
        var eventDrawingResult = await raffleService.ExecuteRaffleAsync(eventRaffleConfig,
            perEventPrizePrizeDescriptions,
            eventRaceResults,
            eventPrizePreviousWinners,
            prizeLimitChecker,
            round => eventResourceIdBuilder.Copy().WithEventDrawingRound(round.ToString()));
        drawingResults.Add(eventDrawingResult);
        logger.LogInformation($"event {eventDrawingResult.winners.Count} won");
        logger.LogInformation($"event {eventDrawingResult.notAwarded.Count} not-awarded");

        eventDrawingActivity?.Dispose();

        var raffleResult = new RaffleResult
        {
            drawings = drawingResults,
            resourceId = eventResourceIdBuilder.Build(),
            name = ResourceIdBuilder.NormalizeEventName(runConfiguration.eventName),
            season = runConfiguration.season,
            configurationName = runConfiguration.name,
            trackName = runConfiguration.trackName,
        };

        foreach (var writer in raffleResultWriters)
        {
            await writer.WriteAsync(raffleResult, token);
        }

        completionToken.Complete();
    }
}