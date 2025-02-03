namespace Deluxxe.IO;

public record JsonRaffleSerializerOptions
{
    public string outputDirectory { get; init; }
    public bool shouldOverwrite { get; init; }
}