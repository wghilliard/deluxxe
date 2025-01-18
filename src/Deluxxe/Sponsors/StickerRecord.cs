namespace Deluxxe.Sponsors;

public record StickerParseResult
{
    public required IDictionary<string, IDictionary<string, bool>> CarToStickerMapping { get; init; }
    public required IDictionary<string, string> DriverToCarMapping { get; init; }
}