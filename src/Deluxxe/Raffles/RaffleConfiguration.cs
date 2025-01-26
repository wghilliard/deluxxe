namespace Deluxxe.Raffles;

public record RaffleConfiguration
{
    public required int MaxRounds { get; init; }
    public required string Season { get; init; }
    public required DrawingType DrawingType { get; init; }
    public required Uri StickerMapUri { get; init; }
}