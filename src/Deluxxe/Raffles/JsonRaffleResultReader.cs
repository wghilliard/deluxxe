using System.Text.Json;
using Deluxxe.IO;

namespace Deluxxe.Raffles;

public class JsonRaffleResultReader : IRaffleResultReader
{
    public async Task<RaffleResult?> ReadAsync(Uri uri,CancellationToken cancellationToken)
    {
        var fileHandle = FileUriParser.Parse(uri).First();
        await using Stream stream = new FileStream(fileHandle.FullName, FileMode.OpenOrCreate);
        return await JsonSerializer.DeserializeAsync<RaffleResult>(stream, cancellationToken: cancellationToken);    }
}