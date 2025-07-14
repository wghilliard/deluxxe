namespace Deluxxe.IO;

public record DeluxxeSerializerOptions
{
    public bool shouldOverwrite { get; init; }

    public bool writeIntermediates { get; init; }
}