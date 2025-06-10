using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Deluxxe.IO;
using Deluxxe.Raffles;

namespace Deluxxe.RaceResults;

public class RaceResultsService(SpeedHiveClient speedHiveClient, RaffleSerializerOptions serializerOptions)
{
    public async Task<IList<Driver>> GetAllDriversAsync(Uri raceResultUri, Dictionary<string, string> conditions, CancellationToken cancellationToken)
    {
        IList<RaceResultRecord> raceResults;
        if (raceResultUri.IsFile)
        {
            raceResults = (await FileUriParser.ParseAndDeserializeSingleAsync<RaceResultResponse>(raceResultUri, extensions: ["json"], cancellationToken))!.rows;
        }
        else
        {
            var name = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raceResultUri.AbsoluteUri))).Replace("/", "_");
            var filePath = Path.Combine(serializerOptions.outputDirectory, $"{name}-source-race-results.json");
            var file = new FileInfo(filePath);

            if (file.Exists)
            {
                raceResults = (await FileUriParser.ParseAndDeserializeSingleAsync<RaceResultResponse>(new Uri($"file://{file.FullName}"), extensions: ["json"], cancellationToken))!.rows;
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
}