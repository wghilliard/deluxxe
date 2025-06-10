namespace Deluxxe.IO;

public record RaffleSerializerOptions
{
    public string outputDirectory { get; init; }
    public bool shouldOverwrite { get; init; }
    
    public bool writeIntermediates { get; init; }
}