using System.Text;
using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class CsvRaffleResultWriter(ILogger<CsvRaffleResultWriter> logger, RaffleSerializerOptions options) : IRaffleResultWriter
{
    public async Task<Uri> WriteAsync(RaffleResult result, CancellationToken cancellationToken)
    {
        var fileName = $"{result.season}-{result.name}-results.csv";
        var file = new FileInfo(Path.Combine(Path.GetFullPath(options.outputDirectory), fileName));

        if (file.Exists && options.shouldOverwrite)
        {
            logger.LogInformation($"deleting previous file at {file.FullName}");
            file.Delete();
        }
        
        logger.LogInformation($"writing to {file.FullName}");
        await using var stream = new FileStream(file.FullName, FileMode.OpenOrCreate);
        var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteLineAsync("drawing type, name, resource id");

        foreach (var drawing in result.drawings)
        {
            foreach (var winner in drawing.winners)
            {
                await writer.WriteLineAsync($"{drawing.drawingType}, {winner.candidate.name}, {winner.resourceId}");
            }
        }
        
        await writer.FlushAsync(cancellationToken);
        return FileUriParser.Generate(options.outputDirectory, fileName);
    }
}