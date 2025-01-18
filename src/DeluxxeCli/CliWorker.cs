using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deluxxe.ModelsV3;
using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class CliWorker(ActivitySource activitySource, ILogger<CliWorker> logger, IServiceProvider serviceProvider) : BackgroundService
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
            .Select(row => new RaceResult()
            {
                Driver = new Driver()
                {
                    Name = row.name,
                    Email = string.Empty,
                    CarNumber = row.startNumber
                },
                CarClass = row.resultClass,
                Gap = row.status
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
                    Description = record.description,
                    SponsorName = record.name,
                });
            }
        }

        // 3. get previous results
        // TODO
        var previousWinners = new List<PrizeWinner<PrizeDescription>>();

        // 4. draw first event
        var raffleLogger = serviceProvider.GetRequiredService<ILogger<PrizeRaffle<PrizeDescription>>>();
        var satRaffle = new PrizeRaffle<PrizeDescription>(raffleLogger, activitySource, stickerManager);

        var (satWinners, satNotAwarded) = satRaffle.DrawPrizes(perRacePrizePrizeDescriptions, satRaceResults, previousWinners);
        // previousWinners.AddRange(satWinners);
        logger.LogInformation($"sat {satWinners.Count} won");
        logger.LogInformation($"sat {satNotAwarded.Count} not-awarded");

        Stream sunRaceResultsStream = new FileStream(Path.Combine("Data", "sun-race-results.json"), FileMode.Open);
        using var sunRaceResultsStreamReader = new StreamReader(sunRaceResultsStream, Encoding.UTF8);
        var sunRaceResultResponse = JsonSerializer.Deserialize<RaceResultResponse>(await sunRaceResultsStreamReader.ReadToEndAsync(token));
        var sunRaceResults = sunRaceResultResponse.rows
            .Where(row => row.status != "DNS")
            .Where(row => row.resultClass == "PRO3")
            .Select(row => new RaceResult()
            {
                Driver = new Driver()
                {
                    Name = row.name,
                    Email = string.Empty,
                    CarNumber = row.startNumber
                },
                CarClass = row.resultClass,
                Gap = row.status
            }).ToList();

        var sunRaffle = new PrizeRaffle<PrizeDescription>(raffleLogger, activitySource, stickerManager);

        var (sunWinners, sunNotAwarded) = sunRaffle.DrawPrizes(perRacePrizePrizeDescriptions, sunRaceResults, previousWinners);
        // previousWinners.AddRange(sunWinners);

        logger.LogInformation($"sun {sunWinners.Count} won");
        logger.LogInformation($"sun {sunNotAwarded.Count} not-awarded");

        var eventRaceResults = new List<RaceResult>();
        eventRaceResults.AddRange(satRaceResults);
        eventRaceResults.AddRange(sunRaceResults);

        var perEventPrizePrizeDescriptions = new List<PrizeDescription>();
        foreach (var record in sponsorRecords.perEventPrizes)
        {
            for (int count = 0; count < record.count; count++)
            {
                perEventPrizePrizeDescriptions.Add(new PrizeDescription()
                {
                    Description = record.description,
                    SponsorName = record.name,
                });
            }
        }

        var eventRaffle = new PrizeRaffle<PrizeDescription>(raffleLogger, activitySource, stickerManager);
        var (eventWinners, eventNotAwarded) = eventRaffle.DrawPrizes(perEventPrizePrizeDescriptions, eventRaceResults, previousWinners);
        logger.LogInformation($"event {eventWinners.Count} won");
        logger.LogInformation($"event {eventNotAwarded.Count} not-awarded");
    }
}