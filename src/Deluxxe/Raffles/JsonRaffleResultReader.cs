using System.Text.Json;
using Deluxxe.IO;

namespace Deluxxe.Raffles;

public class JsonRaffleResultReader : IRaffleResultReader
{
    public async Task<RaffleResult?> ReadAsync(Uri uri, CancellationToken cancellationToken)
    {
        var fileInfo = FileUriParser.Parse(uri).First();
        await using Stream stream = new FileStream(fileInfo.FullName, FileMode.OpenOrCreate);
        return await JsonSerializer.DeserializeAsync<RaffleResult>(stream, cancellationToken: cancellationToken);
    }
}