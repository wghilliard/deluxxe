using Deluxxe.Resources;

namespace Deluxxe.Raffles;

public record DrawingConfiguration
{
    public required DrawingType DrawingType { get; init; }
    public required string Season { get; init; }
    public required ResourceIdBuilder ResourceIdBuilder { get; init; }
}