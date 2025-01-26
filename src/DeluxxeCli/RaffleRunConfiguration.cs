namespace DeluxxeCli;

public record RaffleRunConfiguration
{
    public string name { get; set; }
    public string season { get; init; }
    public string eventName { get; init; }
    public string eventId { get; init; }
    public Uri stickerMapUri { get; init; }
    public Uri prizeDescriptionUri { get; init; }
    public List<RaceResultConfiguration> raceResults { get; init; }
    public string outputDirectory { get; init; }
    public bool shouldOverwrite { get; init; }
    public string previousResultsPattern { get; init; }
    public Dictionary<string, string> conditions { get; init; }
}

public record RaceResultConfiguration
{
    public string sessionName { get; init; }
    public string sessionId { get; init; }
    public Uri raceResultUri { get; init; }
}