using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

using System.Text.Json;

public class JsonRaffleResultWriter(ILogger<JsonRaffleResultWriter> logger, string outputDirectory, bool overWrite = false) : IRaffleResultWriter
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public async Task<bool> WriteAsync(RaffleResult result, CancellationToken cancellationToken = default)
    {
        var file = new FileInfo(Path.Combine(Path.GetFullPath(outputDirectory), $"{result.season}-{result.name}-results.json"));

        if (file.Exists)
        {
            logger.LogInformation($"deleting previous file at {file.FullName}");
            file.Delete();
        }

        logger.LogInformation($"writing to {file.FullName}");
        await using Stream stream = new FileStream(file.FullName, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(stream, result, cancellationToken: cancellationToken, options: _options);
        stream.Close();
        return true;
    }
}