using System.Diagnostics;
using System.Text.Json;
using Deluxxe.RaceResults;
using Deluxxe.RaceResults.SpeedHive;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class CreateEventCliWorker(
    ActivitySource activitySource,
    ILogger<CreateEventCliWorker> logger,
    CompletionToken completionToken,
    CreateEventOptions options,
    SpeedHiveClient speedHiveClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity(nameof(CreateEventCliWorker));

        var yearMatch = System.Text.RegularExpressions.Regex.Match(options.Date, @"(\d{4})");
        var season = yearMatch.Success ? yearMatch.Groups[1].Value : DateTime.Now.Year.ToString();

        var outputDir = new DirectoryInfo(options.OutputDir);
        var eventPath = new DirectoryInfo(Path.Combine(outputDir.FullName, $"{options.Date}-{options.EventName}"));
        var deluxxePath = new DirectoryInfo(Path.Combine(eventPath.FullName, "deluxxe"));
        var previousResultsPath = new DirectoryInfo(Path.Combine(deluxxePath.FullName, "previous-results"));
        var collateralPath = new DirectoryInfo(Path.Combine(eventPath.FullName, "collateral"));

        logger.LogInformation($"Creating directory structure for event '{options.EventName}'");
        eventPath.Create();
        deluxxePath.Create();
        previousResultsPath.Create();
        collateralPath.Create();

        int? eventId = options.EventId;
        List<SpeedHiveSession> raceSessions;

        if (!string.IsNullOrEmpty(options.MylapsAccountId))
        {
            logger.LogInformation($"Querying SpeedHive for events...");
            var events = await speedHiveClient.GetEventsAsync(options.MylapsAccountId, token);
            if (events != null)
            {
                var latestEvent = events
                    .Where(e => string.Equals(e.Organization.Name, "Cascade Sports Car Club", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(e.Organization.Name, "IRDC", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.StartDate)
                    .FirstOrDefault();

                if (latestEvent != null)
                {
                    logger.LogInformation($"Found latest event: {latestEvent.Name} (ID: {latestEvent.Id})");
                    eventId = latestEvent.Id;
                }
                else
                {
                    throw new ArgumentException("No valid events found for the specified Mylaps account ID. Please check the account ID or provide a valid event ID.");
                }
            }
        }

        if (!eventId.HasValue)
        {
            throw new ArgumentException("Could not find a valid event ID. Please provide a valid Mylaps account ID or specify an event ID.");
        }

        var eventDetails = await speedHiveClient.GetEventDetailsAsync(eventId.Value, token);
        if (eventDetails != null)
        {
            raceSessions = eventDetails.Sessions.Groups
                .Where(g => string.Equals(g.Name, "group 1", StringComparison.OrdinalIgnoreCase))
                .SelectMany(g => g.Sessions)
                .Where(s => string.Equals(s.Type, "race", StringComparison.OrdinalIgnoreCase))
                .ToList();
            logger.LogInformation($"Found {raceSessions.Count} Group 1 race sessions.");
        }
        else
        {
            throw new ArgumentException($"Could not find event details for event ID {eventId.Value}. Please check the event ID.");
        }

        var carMappingFile = outputDir.GetFiles("car-to-sticker-mapping-*.csv").MaxBy(f => f.Name);
        var prizeFile = outputDir.GetFiles("prize-descriptions-*.json").MaxBy(f => f.Name);

        if (carMappingFile != null)
        {
            var linkPath = Path.Combine(deluxxePath.FullName, carMappingFile.Name);
            if (!File.Exists(linkPath))
            {
                File.CreateSymbolicLink(linkPath, carMappingFile.FullName);
                logger.LogInformation($"Linked {carMappingFile.Name}");
            }
        }
        else
        {
            throw new FileNotFoundException($"Could not find car mapping file.");
        }

        if (prizeFile != null)
        {
            var linkPath = Path.Combine(deluxxePath.FullName, prizeFile.Name);
            if (!File.Exists(linkPath))
            {
                File.CreateSymbolicLink(linkPath, prizeFile.FullName);
                logger.LogInformation($"Linked {prizeFile.Name}");
            }
        }
        else
        {
            throw new FileNotFoundException($"Could not find prize mapping file.");
        }

        var templatePath = Path.Combine(outputDir.FullName, "event-template.json");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Event template file not found. Please ensure 'event-template.json' exists in the output directory.");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath, token);
        var templateConfig = JsonSerializer.Deserialize<RaffleRunConfiguration>(templateContent);

        if (templateConfig == null)
        {
            logger.LogError("Failed to parse event template JSON.");
            completionToken.Complete();
            return;
        }

        var normalizedEventName = options.EventName.ToLower().Replace(' ', '-');

        List<RaceResultConfiguration> raceResults = new();
        foreach (var session in raceSessions)
        {
            raceResults.Add(new RaceResultConfiguration()
            {
                sessionName = session.Name.Split(' ')[0].ToLower(),
                sessionId = session.Id.ToString(),
                startTime = session.StartTime
            });
        }

        var newConfig = templateConfig with
        {
            name = season + "-" + normalizedEventName,
            season = season,
            eventName = options.EventName,
            eventId = eventId.Value.ToString(),
            stickerMapUri = new Uri($"file://local/{carMappingFile.Name}"),
            prizeDescriptionUri = new Uri($"file://local/{prizeFile.Name}"),
            raceResults = raceResults,
            trackName = eventDetails.Location.Name
        };

        var configPath = Path.Combine(deluxxePath.FullName, "deluxxe.json");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(newConfig, jsonOptions), token);
        logger.LogInformation("Created deluxxe.json config.");


        foreach (var dir in outputDir.GetDirectories())
        {
            var eventDeluxxePath = new DirectoryInfo(Path.Combine(dir.FullName, "deluxxe"));
            if (eventDeluxxePath.Exists && !dir.Name.Contains("test"))
            {
                foreach (var file in eventDeluxxePath.GetFiles("*-results.json"))
                {
                    if (!file.Name.Contains('='))
                    {
                        var linkPath = Path.Combine(previousResultsPath.FullName, file.Name);
                        if (!File.Exists(linkPath))
                        {
                            File.CreateSymbolicLink(linkPath, file.FullName);
                        }
                    }
                }
            }
        }

        logger.LogInformation("Event creation process complete.");
        completionToken.Complete();
    }
}