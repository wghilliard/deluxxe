namespace Deluxxe.Raffles;

public record RaffleExecutionConfiguration
{
    public required int MaxRounds { get; init; }
    public required bool ClearHistoryIfNoCandidates { get; init; }
    public required string Season { get; init; }
    public required DrawingType DrawingType { get; init; }
    public required Uri StickerMapUri { get; init; }
}