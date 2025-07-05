using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Deluxxe.IO;
using Deluxxe.Raffles;

namespace Deluxxe.RaceResults;

public class RaceResultsService(SpeedHiveClient speedHiveClient, RaffleSerializerOptions serializerOptions, IDirectoryManager directoryManager)
{
    public Task<IList<Driver>> GetAllDriversAsync(string sessionId, Dictionary<string, string> conditions, CancellationToken cancellationToken)
    {
        return GetAllDriversAsync(SpeedHiveClient.GetApiJsonUrlFromSessionId(sessionId), conditions, cancellationToken);
    }

    private async Task<IList<Driver>> GetAllDriversAsync(Uri raceResultUri, Dictionary<string, string> conditions, CancellationToken cancellationToken)
    {
        IList<RaceResultRecord> raceResults;
        if (raceResultUri.IsFile)
        {
            raceResults = (await FileUriParser.ParseAndDeserializeSingleAsync<RaceResultResponse>(raceResultUri, directoryManager, extensions: ["json"], cancellationToken))!.rows;
        }
        else
        {
            var name = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raceResultUri.AbsoluteUri))).Replace("/", "_");
            var filePath = Path.Combine(directoryManager.deluxxeDir.FullName, $"{name}-source-race-results.json");
            var file = new FileInfo(filePath);

            if (file.Exists)
            {
                raceResults = (await FileUriParser.ParseAndDeserializeSingleAsync<RaceResultResponse>(new Uri($"file://{file.FullName}"), directoryManager, extensions: ["json"], cancellationToken))!.rows;
            }
            else
            {
                var response = await speedHiveClient.GetResultsFromJsonUrl(raceResultUri, cancellationToken);
                raceResults = new List<RaceResultRecord>(response.rows);
                if (serializerOptions.writeIntermediates)
                {
                    await using var stream = file.OpenWrite();
                    await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(response, options: new JsonSerializerOptions()
                    {
                        IndentSize = 2,
                        WriteIndented = true
                    }), cancellationToken);
                }
            }
        }

        var loadedConditions = conditions.Select(pair => new Condition(pair.Key, pair.Value)).ToList();
        return raceResults.Where(result => loadedConditions.All(condition => condition.IsSatisfied(result)))
            .Select(result => new Driver
            {
                name = result.name,
                carNumber = result.startNumber
            }).ToList();
    }

    public Task<FileInfo> SaveResultsAsPdfAsync(string sessionId, CancellationToken cancellationToken)
    {
        return SaveResultsAsPdfAsync(SpeedHiveClient.GetUiUrlFromSessionId(sessionId), cancellationToken);
    }

    private async Task<FileInfo> SaveResultsAsPdfAsync(Uri raceResultUiUrl, CancellationToken cancellationToken)
    {
        var name = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raceResultUiUrl.AbsoluteUri))).Replace("/", "_");
        var filePath = Path.Combine(directoryManager.collateralDir.FullName, $"{name}-race-results.pdf");
        var file = new FileInfo(filePath);

        if (file.Exists)
        {
            return file;
        }

        var pdfBytes = await speedHiveClient.GetResultsAsPdfAsync(raceResultUiUrl, cancellationToken);
        await File.WriteAllBytesAsync(file.FullName, pdfBytes, cancellationToken);

        return file;
    }
}