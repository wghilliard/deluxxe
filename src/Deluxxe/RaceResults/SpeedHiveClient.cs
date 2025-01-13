using System.Diagnostics;
using System.Text;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace Deluxxe.RaceResults
{
    public class SpeedHiveClient(ActivitySource activitySource, IHttpClientFactory httpClientFactory)
    {
        private const string BaseUrl = "https://eventresults-api.speedhive.com/api/{0}/eventresults/sessions/{1}/csv";
        private const string ApiVersion = "v0.2.3";

        private readonly CsvParser<RaceResultRecord> _parser = new(new CsvParserOptions(true, ','), new CsvRaceResultRecordMapping());

        public async Task<IEnumerable<RaceResultRecord>> GetResultsFromUrl(Uri url, CancellationToken token = default)
        {
            using var client = httpClientFactory.CreateClient("SpeedHiveClient");
            client.DefaultRequestHeaders.Add("Origin", "https://speedhive.mylaps.com");
            client.DefaultRequestHeaders.Add("Referer", "https://speedhive.mylaps.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:133.0)");
            using var response = await client.GetAsync(url, token);

            if (response.IsSuccessStatusCode)
            {
                return await ParseAsync(response.Content.ReadAsStreamAsync(token), token);
            }

            throw new HttpRequestException($"Unable to get race results from url: {url}, response: {response}, responseCode: {response.StatusCode}");
        }

        public async Task<IEnumerable<RaceResultRecord>> ParseAsync(Task<Stream> input, CancellationToken token = default)
        {
            var results = _parser.ReadFromStream(await input, Encoding.Unicode, detectEncodingFromByteOrderMarks: true)
                .ToList();

            return results.Select(result => result.Result);
        }

        public static Uri GetApiUrlFromUiUrl(Uri uiUrl)
        {
            var path = uiUrl.PathAndQuery.Split('?')[0].Split('/');
            if (path.Length < 2)
            {
                throw new HttpRequestException($"Unable to get api url from UI url: {uiUrl}");
            }

            // /sessions/8939619?one=two
            var sessionId = path[2];

            return new Uri(string.Format(BaseUrl, ApiVersion, sessionId));
        }

        public static Uri GetApiUrlFromSessionId(string sessionId)
        {
            return new Uri(string.Format(BaseUrl, ApiVersion, sessionId));
        }
    }

    internal class CsvRaceResultRecordMapping : CsvMapping<RaceResultRecord>
    {
        public CsvRaceResultRecordMapping()
        {
            MapProperty(0, x => x.Position);
            MapProperty(1, x => x.StartNumber);
            MapProperty(2, x => x.Competitor);
            MapProperty(3, x => x.Class);
            MapProperty(4, x => x.TotalTime);
            MapProperty(5, x => x.Diff);
            MapProperty(6, x => x.Laps);
            MapProperty(7, x => x.BestLap);
            MapProperty(8, x => x.BestLapNumber);
            MapProperty(9, x => x.BestSpeed);
        }
    }
}