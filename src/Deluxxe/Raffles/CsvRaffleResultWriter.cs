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
        await writer.WriteLineAsync("event name, drawing type, name, prize description, resource id");

        foreach (var drawing in result.drawings)
        {
            var sortedPrizes = drawing.winners.ToArray();
            Array.Sort(sortedPrizes, (a ,b )=> String.Compare(ToPrettyPrizeString(a.prizeDescription), ToPrettyPrizeString(b.prizeDescription), StringComparison.Ordinal));
            foreach (var winner in sortedPrizes)
            {
                await writer.WriteLineAsync($"{result.configurationName}, {drawing.drawingType}, {winner.candidate.name}, {ToPrettyPrizeString(winner.prizeDescription)} , {winner.resourceId}");
            }
        }
        
        await writer.FlushAsync(cancellationToken);
        return FileUriParser.Generate(options.outputDirectory, fileName);
    }

    private static string ToPrettyPrizeString(PrizeDescription prizeDescription)
    {
        return $"[{prizeDescription.sponsorName}][{prizeDescription.description}]";
    }
}