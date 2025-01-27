using Deluxxe.IO;

namespace DeluxxeCli;

public record RaffleRunConfiguration
{
    public required string name { get; set; }
    public required string season { get; init; }
    public required string eventName { get; init; }
    public required string eventId { get; init; }
    public required Uri stickerMapUri { get; init; }
    public required Uri prizeDescriptionUri { get; init; }
    public required List<RaceResultConfiguration> raceResults { get; init; }

    public required Uri previousResultsUri { get; init; }
    public required Dictionary<string, string> conditions { get; init; }

    public required JsonRaffleSerializerOptions jsonOptions { get; init; }
}

public record RaceResultConfiguration
{
    public string sessionName { get; init; }
    public string sessionId { get; init; }
    public Uri raceResultUri { get; init; }
}