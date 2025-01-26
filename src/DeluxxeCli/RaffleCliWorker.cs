using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class RaffleCliWorker(
    ActivitySource activitySource,
    ILogger<RaffleCliWorker> logger,
    IServiceProvider serviceProvider,
    CompletionToken completionToken,
    RaffleRunConfiguration runConfiguration,
    RaffleService raffleService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity("deluxxe-cli");
        var eventResourceIdBuilder = new ResourceIdBuilder().WithSeason(runConfiguration.season).WithEvent(runConfiguration.eventName, runConfiguration.eventId);

        // 2. get the prize descriptions
        // todo - validate the prize json
        Stream sponsorRecordStream = new FileStream(FileUriParser.Parse(runConfiguration.prizeDescriptionUri)!.FullName, FileMode.Open);
        using var sponsorRecordStreamReader = new StreamReader(sponsorRecordStream, Encoding.UTF8);
        var sponsorRecords = JsonSerializer.Deserialize<SponsorRecords>(await sponsorRecordStreamReader.ReadToEndAsync(token));

        var perRacePrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords!.perRacePrizes)
        {
            for (int count = 0; count < record.count; count++)
            {
                perRacePrizePrizeDescriptions.Add(new PrizeDescription()
                {
                    description = record.description,
                    sponsorName = record.name,
                    sku = record.sku
                });
            }
        }

        // 3. get previous results
        // TODO
        var previousWinners = new List<PrizeWinner>();
        var eventRaceResults = new List<Driver>();
        var drawingResults = new List<DrawingResult>();
        var racePrizePreviousWinners = new List<PrizeWinner>();
        racePrizePreviousWinners.AddRange(previousWinners);
        foreach (var result in runConfiguration.raceResults)
        {
            // 4. get the race results
            Stream raceResultsStream = new FileStream(FileUriParser.Parse(result.raceResultUri)!.FullName, FileMode.Open);
            using var raceResultsStreamReader = new StreamReader(raceResultsStream, Encoding.UTF8);
            var raceResultResponse = JsonSerializer.Deserialize<RaceResultResponse>(await raceResultsStreamReader.ReadToEndAsync(token));
            var raceResults = raceResultResponse!.rows
                .Where(row => row.status != "DNS")
                .Where(row => row.resultClass == "PRO3")
                .Select(row => new Driver
                {
                    name = row.name,
                    carNumber = row.startNumber
                }).ToList();
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
                round => eventResourceIdBuilder.Copy().WithRaceDrawingRound(result.sessionName, result.sessionId, round.ToString()));
            drawingResults.Add(drawingResult);
            racePrizePreviousWinners.AddRange(drawingResult.winners);
            logger.LogInformation($"sat {drawingResult.winners.Count} won");
            logger.LogInformation($"sat {drawingResult.notAwarded.Count} not-awarded");
        }

        var perEventPrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords.perEventPrizes)
        {
            for (int count = 0; count < record.count; count++)
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
        };

        await new JsonRaffleResultWriter(serviceProvider.GetRequiredService<ILogger<JsonRaffleResultWriter>>(), 
                runConfiguration.outputDirectory, 
                runConfiguration.shouldOverwrite)
            .WriteAsync(raffleResult, token);

        completionToken.Complete();
    }
}