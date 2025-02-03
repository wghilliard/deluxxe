using Deluxxe.IO;

namespace Deluxxe.Raffles;

public class PreviousWinnerLoader(IRaffleResultReader resultReader)
{
    public async Task<IList<PrizeWinner>> LoadAsync(Uri previousResultsUri, CancellationToken cancellationToken)
    {
        var previousResults = new List<PrizeWinner>();
        var fileHandles = FileUriParser.Parse(previousResultsUri);
        foreach (var fileHandle in fileHandles)
        {
            var raffleResult = await resultReader.ReadAsync(new Uri(fileHandle.FullName), cancellationToken);
            foreach (var drawing in raffleResult!.drawings)
            {
                previousResults.AddRange(drawing.winners);
            }
        }

        return previousResults;
    }
}