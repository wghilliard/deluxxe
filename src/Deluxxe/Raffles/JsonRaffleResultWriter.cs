using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

using System.Text.Json;

public class JsonRaffleResultWriter(ILogger<JsonRaffleResultWriter> logger, DeluxxeSerializerOptions options, IDirectoryManager directoryManager) : IRaffleResultWriter
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public async Task<Uri> WriteAsync(RaffleResult result, CancellationToken cancellationToken = default)
    {
        var fileName = $"{result.season}-{result.name}-results.json";
        var file = new FileInfo(Path.Combine(directoryManager.deluxxeDir.FullName, fileName));

        if (file.Exists && options.shouldOverwrite)
        {
            logger.LogInformation($"deleting previous file at {file.FullName}");
            file.Delete();
        }

        logger.LogInformation($"writing to {file.FullName}");
        await using var stream = new FileStream(file.FullName, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(stream, result, cancellationToken: cancellationToken, options: _options);
        return FileUriParser.Generate(directoryManager.deluxxeDirRelative, fileName);
    }
}