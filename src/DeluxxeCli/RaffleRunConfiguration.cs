using Deluxxe.IO;
using Deluxxe.Raffles;

namespace DeluxxeCli;

public record RaffleRunConfiguration
{
    public required string name { get; set; }
    public required string season { get; init; }
    public required string eventName { get; init; }
    public required string eventId { get; init; }
    public string? trackName { get; init; }
    public required Uri stickerMapUri { get; init; }
    public required string stickerMapSchemaVersion { get; init; }
    public required Uri prizeDescriptionUri { get; init; }
    public required List<RaceResultConfiguration> raceResults { get; init; }
    public required Dictionary<string, string> conditions { get; init; }
    public required DeluxxeSerializerOptions serializerOptions { get; init; }
    public required RaffleConfiguration raffleConfiguration { get; init; }
}

public struct RaceResultConfiguration
{
    public string sessionName { get; init; }
    public required string sessionId { get; init; }

    public string? startTime { get; init; }
}