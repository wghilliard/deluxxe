using System.Diagnostics;
using Deluxxe.IO;

namespace Deluxxe.Raffles;

public class PreviousWinnerLoader(ActivitySource activitySource, IRaffleResultReader resultReader, IDirectoryManager directoryManager)
{
    public async Task<IList<PrizeWinner>> LoadAsync(CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity("Loading previous results");
        var previousResults = new List<PrizeWinner>();
        var fileHandles = directoryManager.previousResultsDir.GetFiles().ToList();
        activity?.AddTag("fileCount", fileHandles.Count);
        foreach (var fileHandle in fileHandles)
        {
            using var fileActivity = activitySource.StartActivity("Reading previous results file");
            fileActivity?.AddTag("fileName", fileHandle.FullName);
            var resultsFound = 0;
            var raffleResult = await resultReader.ReadAsync(new Uri(fileHandle.FullName), cancellationToken);
            foreach (var drawing in raffleResult.drawings)
            {
                previousResults.AddRange(drawing.winners);
                resultsFound += drawing.winners.Count;
            }

            fileActivity?.AddTag("resultsCount", resultsFound);
        }

        return previousResults;
    }
}