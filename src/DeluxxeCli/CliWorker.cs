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

public class CliWorker(ActivitySource activitySource, ILogger<CliWorker> logger, IServiceProvider serviceProvider, CompletionToken completionToken) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity("deluxxe-cli");
        var season = DateTimeOffset.UtcNow.Year;
        const string eventName = "IRDC - Spring into Summer";
        const string eventId = "2609223";
        var eventResourceIdBuilder = new ResourceIdBuilder().WithSeason(season).WithEvent(eventName, eventId);

        // 1. get the sticker map
        var uri = new Uri("file://Data/car-to-sticker-mapping.csv");
        var stickerProvider = serviceProvider.GetRequiredService<StickerProviderUriResolver>();
        var stickerManager = await stickerProvider.GetStickerManager(uri);

        // 2. get the race results
        Stream satRaceResultsStream = new FileStream(Path.Combine("Data", "sat-race-results.json"), FileMode.Open);
        using var satRaceResultsStreamReader = new StreamReader(satRaceResultsStream, Encoding.UTF8);
        var satRaceResultResponse = JsonSerializer.Deserialize<RaceResultResponse>(await satRaceResultsStreamReader.ReadToEndAsync(token));
        var satRaceResults = satRaceResultResponse.rows
            .Where(row => row.status != "DNS")
            .Where(row => row.resultClass == "PRO3")
            .Select(row => new Driver()
            {
                name = row.name,
                carNumber = row.startNumber
            }).ToList();

        // 3. get the prize descriptions
        // todo - validate the prize json
        Stream sponsorRecordStream = new FileStream(Path.Combine("Data", "prize-descriptions.json"), FileMode.Open);
        using var sponsorRecordStreamReader = new StreamReader(sponsorRecordStream, Encoding.UTF8);
        var sponsorRecords = JsonSerializer.Deserialize<SponsorRecords>(await sponsorRecordStreamReader.ReadToEndAsync(token));

        var perRacePrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords.perRacePrizes)
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

        // 4. draw first event
        var randomSeed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var raffleLogger = serviceProvider.GetRequiredService<ILogger<PrizeRaffle>>();
        var satRaffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, (int)randomSeed);

        var satDrawingResult = satRaffle.DrawPrizes(perRacePrizePrizeDescriptions, satRaceResults, previousWinners, new DrawingConfiguration()
        {
            DrawingType = DrawingType.Race,
            season = season,
            resourceIdBuilder = eventResourceIdBuilder.Copy().WithRaceDrawing("saturday-race-1", "1")
            // resourceIdBuilder = "season/2025/event/summer-into-spring/2609223/session/sat-race/8939601"
        });
        // previousWinners.AddRange(satWinners);
        logger.LogInformation($"sat {satDrawingResult.winners.Count} won");
        logger.LogInformation($"sat {satDrawingResult.notAwarded.Count} not-awarded");

        Stream sunRaceResultsStream = new FileStream(Path.Combine("Data", "sun-race-results.json"), FileMode.Open);
        using var sunRaceResultsStreamReader = new StreamReader(sunRaceResultsStream, Encoding.UTF8);
        var sunRaceResultResponse = JsonSerializer.Deserialize<RaceResultResponse>(await sunRaceResultsStreamReader.ReadToEndAsync(token));
        var sunRaceResults = sunRaceResultResponse.rows
            .Where(row => row.status != "DNS")
            .Where(row => row.resultClass == "PRO3")
            .Select(row => new Driver()
            {
                name = row.name,
                carNumber = row.startNumber
            }).ToList();

        randomSeed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sunRaffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, (int)randomSeed);

        var sunDrawingResult = sunRaffle.DrawPrizes(perRacePrizePrizeDescriptions, sunRaceResults, previousWinners, new DrawingConfiguration()
        {
            DrawingType = DrawingType.Race,
            season = season,
            resourceIdBuilder = eventResourceIdBuilder.Copy().WithRaceDrawing("sunday-race-1", "1")
        });
        // previousWinners.AddRange(sunWinners);

        logger.LogInformation($"sun {sunDrawingResult.winners.Count} won");
        logger.LogInformation($"sun {sunDrawingResult.notAwarded.Count} not-awarded");

        var eventRaceResults = new List<Driver>();
        eventRaceResults.AddRange(satRaceResults);
        eventRaceResults.AddRange(sunRaceResults);

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

        randomSeed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var eventRaffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, (int)randomSeed);
        var eventDrawingResult = eventRaffle.DrawPrizes(perEventPrizePrizeDescriptions, eventRaceResults, previousWinners, new DrawingConfiguration()
        {
            DrawingType = DrawingType.Event,
            season = season,
            resourceIdBuilder = eventResourceIdBuilder.Copy().WithEventDrawing("3")
        });
        logger.LogInformation($"event {eventDrawingResult.winners.Count} won");
        logger.LogInformation($"event {eventDrawingResult.notAwarded.Count} not-awarded");

        var raffleResult = new RaffleResult()
        {
            drawings = [satDrawingResult, sunDrawingResult, eventDrawingResult],
            resourceId = eventResourceIdBuilder.Build(),
            name = ResourceIdBuilder.NormalizeEventName(eventName),
            season = season
        };

        await new JsonRaffleResultWriter(serviceProvider.GetRequiredService<ILogger<JsonRaffleResultWriter>>(), "~/tmp").WriteAsync(raffleResult, token);

        completionToken.Complete();
    }
}