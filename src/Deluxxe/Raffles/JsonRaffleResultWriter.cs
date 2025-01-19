using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

using System.Text.Json;

public class JsonRaffleResultWriter(ILogger<JsonRaffleResultWriter> logger, string outputDirectory) : IRaffleResultWriter
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public async Task<bool> WriteAsync(RaffleResult result, CancellationToken cancellationToken = default)
    {
        var fileName = Path.Combine(Directory.GetCurrentDirectory(), $"{result.season}-{result.name}-results.json");
        logger.LogInformation($"writing to {fileName}");
        await using Stream stream = new FileStream(fileName, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(stream, result, cancellationToken: cancellationToken, options: _options);
        stream.Close();
        return true;
    }
}