using System.Text;
using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class CsvRaffleResultWriter(ILogger<CsvRaffleResultWriter> logger, DeluxxeSerializerOptions options, IDirectoryManager directoryManager) : IRaffleResultWriter
{
    public async Task<Uri> WriteAsync(RaffleResult result, CancellationToken cancellationToken)
    {
        var fileName = $"{result.season}-{result.name}-results.csv";
        var file = new FileInfo(Path.Combine(directoryManager.deluxxeDir.FullName, fileName));
        if (file.Exists && options.shouldOverwrite)
        {
            logger.LogInformation($"deleting previous file at {file.FullName}");
            file.Delete();
        }

        logger.LogInformation("writing to {fileFullName}", file.FullName);
        await using var stream = new FileStream(file.FullName, FileMode.OpenOrCreate);
        var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteLineAsync("event name, drawing type, name, sponsor, prize description, prize unique id");

        foreach (var drawing in result.drawings)
        {
            var sortedPrizes = drawing.winners.ToArray();
            Array.Sort(sortedPrizes, (a, b) => string.Compare(a.candidate.name, b.candidate.name, StringComparison.Ordinal));
            foreach (var winner in sortedPrizes)
            {
                await writer.WriteLineAsync(
                    $"{result.configurationName}, {drawing.drawingType}, {winner.candidate.name}, {winner.prizeDescription.sponsorName}, {winner.prizeDescription.description}, {winner.resourceId}");
            }
        }

        await writer.FlushAsync(cancellationToken);
        return FileUriParser.Generate(directoryManager.deluxxeDirRelative, fileName);
    }
}