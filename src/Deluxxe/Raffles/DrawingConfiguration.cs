using Deluxxe.Resources;

namespace Deluxxe.Raffles;

public record DrawingConfiguration
{
    public required DrawingType DrawingType { get; init; }
    public required int season { get; init; }
    public required ResourceIdBuilder resourceIdBuilder { get; init; }
}