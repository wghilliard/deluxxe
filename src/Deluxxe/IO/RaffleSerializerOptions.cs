namespace Deluxxe.IO;

public record RaffleSerializerOptions
{
    public bool shouldOverwrite { get; init; }
    
    public bool writeIntermediates { get; init; }
}