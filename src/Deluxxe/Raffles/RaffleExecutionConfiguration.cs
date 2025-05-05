namespace Deluxxe.Raffles;

public struct RaffleExecutionConfiguration
{
    public required int MaxRounds { get; init; }
    public required bool ClearHistoryIfNoCandidates { get; init; }
    public required string Season { get; init; }
    public required DrawingType DrawingType { get; init; }
    public required Uri StickerMapUri { get; init; }
    public required string StickerMapSchemaVersion { get; init; }
    public required int RandomShuffleSeed { get; init; }
    public required int RandomDrawingSeed { get; init; }
    public required bool UseWinningHistory { get; init; }
}