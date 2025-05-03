namespace Deluxxe.Sponsors;

public readonly struct StickerParseResult
{
    public required string schemaVersion { get; init; }
    public required IDictionary<string, IDictionary<string, bool>> carToStickerMapping { get; init; }
    public required IDictionary<string, string> carRentalMap { get; init; }

    public bool IsEmpty()
    {
        return carToStickerMapping.Count == 0
            && carRentalMap.Count == 0;
    }
}