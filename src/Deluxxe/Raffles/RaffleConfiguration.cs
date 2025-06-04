namespace Deluxxe.Raffles;

public record RaffleConfiguration
{
    public int maxRounds { get; init; }
    public bool clearHistoryIfNoCandidates { get; init; }
    public bool allowRentersToWin { get; init; }
    public int randomShuffleSeed { get; init; }
    public int randomDrawingSeed { get; init; }

    public bool filterDriversWithWinningHistory { get; init; }
}