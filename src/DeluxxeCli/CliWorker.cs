using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class CliWorker(ActivitySource activitySource, ILogger<CliWorker> logger, IServiceProvider serviceProvider, CompletionToken completionToken) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        activitySource.StartActivity("deluxxe-cli");
        // 1. get the sticker map
        var stickerProvider = serviceProvider.GetRequiredService<CsvStickerRecordProvider>();
        Stream stickerStream = new FileStream(Path.Combine("Data", "car-to-sticker-mapping.csv"), FileMode.Open);
        var stickerResult = await stickerProvider.ParseCsvAsync(Task.FromResult(stickerStream));
        var stickerManager = new InMemoryStickerManager(stickerResult.CarToStickerMapping);


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
                email = string.Empty,
                carNumber = row.startNumber
            }).ToList();

        // 3. get the prize descriptions
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

        var satDrawingResult = satRaffle.DrawPrizes(perRacePrizePrizeDescriptions, satRaceResults, previousWinners, DrawingType.Race);
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
                email = string.Empty,
                carNumber = row.startNumber
            }).ToList();

        randomSeed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sunRaffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, (int)randomSeed);

        var sunDrawingResult = sunRaffle.DrawPrizes(perRacePrizePrizeDescriptions, sunRaceResults, previousWinners, DrawingType.Race);
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
                });
            }
        }

        randomSeed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var eventRaffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, (int)randomSeed);
        var eventDrawingResult = eventRaffle.DrawPrizes(perEventPrizePrizeDescriptions, eventRaceResults, previousWinners, DrawingType.Event);
        logger.LogInformation($"event {eventDrawingResult.winners.Count} won");
        logger.LogInformation($"event {eventDrawingResult.notAwarded.Count} not-awarded");

        var raffleResult = new RaffleResult()
        {
            drawings = [satDrawingResult, sunDrawingResult, eventDrawingResult],
            eventId = "summer-into-spring"
        };

        await new JsonRaffleResultWriter(serviceProvider.GetRequiredService<ILogger<JsonRaffleResultWriter>>(), "~/tmp").WriteAsync(raffleResult, token);

        completionToken.Complete();
    }
}