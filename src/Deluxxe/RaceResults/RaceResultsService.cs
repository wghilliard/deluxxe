
using Deluxxe.IO;
using Deluxxe.Raffles;

namespace Deluxxe.RaceResults;

public class RaceResultsService(SpeedHiveClient speedHiveClient)
{
    public async Task<IList<Driver>> GetAllDriversAsync(Uri raceResultUri, Dictionary<string, string> conditions, CancellationToken cancellationToken)
    {
        IEnumerable<RaceResultRecord> raceResults;
        if (raceResultUri.IsFile)
        {
            // Stream raceResultsStream = new FileStream(FileUriParser.Parse(raceResultUri, extensions: ["json"])!.First().FullName, FileMode.Open);
            // using var raceResultsStreamReader = new StreamReader(raceResultsStream, Encoding.UTF8);
            // raceResults = JsonSerializer.Deserialize<RaceResultResponse>(await raceResultsStreamReader.ReadToEndAsync(cancellationToken))!.rows;
            raceResults = (await FileUriParser.ParseAndDeserializeSingleAsync<RaceResultResponse>(raceResultUri, extensions: ["json"], cancellationToken))!.rows;
        }
        else
        {
            raceResults = await speedHiveClient.GetResultsFromJsonUrl(raceResultUri, cancellationToken);
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