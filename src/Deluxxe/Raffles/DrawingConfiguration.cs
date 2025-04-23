using Deluxxe.Resources;

namespace Deluxxe.Raffles;

public struct DrawingConfiguration
{
    public required DrawingType DrawingType { get; init; }
    public required string Season { get; init; }
    public required ResourceIdBuilder ResourceIdBuilder { get; init; }
}