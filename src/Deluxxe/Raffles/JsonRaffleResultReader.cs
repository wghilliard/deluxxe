using System.Text.Json;
using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class JsonRaffleResultReader(IDirectoryManager directoryManager, ILogger<JsonRaffleResultReader> logger) : IRaffleResultReader
{
    public async Task<RaffleResult> ReadAsync(Uri uri, CancellationToken cancellationToken)
    {
        var file = FileUriParser.Parse(uri, directoryManager).First();
        await using Stream stream = new FileStream(file.FullName, FileMode.Open);
        return await JsonSerializer.DeserializeAsync<RaffleResult>(stream, cancellationToken: cancellationToken);
    }

    public async Task<RaffleResult> ReadCurrentContextAsync(CancellationToken cancellationToken)
    {
        var file = directoryManager.raffleResultsJsonFile;
        logger.LogInformation(file.FullName);
        await using Stream stream = new FileStream(file.FullName, FileMode.Open);
        return await JsonSerializer.DeserializeAsync<RaffleResult>(stream, cancellationToken: cancellationToken);
    }
}