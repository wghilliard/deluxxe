namespace Deluxxe.Raffles;

public record RaffleConfiguration
{
    public int maxRounds { get; init; }
    public bool clearHistoryIfNoCandidates { get; init; }
    public bool allowRentersToWin { get; init; }
    public int randomSeed { get; init; }
    public bool useWinningHistory { get; init; }
}