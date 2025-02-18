using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Deluxxe.RaceResults
{
    public class SpeedHiveClient(ActivitySource activitySource, IHttpClientFactory httpClientFactory)
    {
        private const string JsonBaseUrl = "https://eventresults-api.speedhive.com/api/{0}/eventresults/sessions/{1}/classification";

        private const string ApiVersion = "v0.2.3";

        private readonly HttpClient _client = httpClientFactory.CreateClient("SpeedHiveClient");

        public async Task<IEnumerable<RaceResultRecord>> GetResultsFromJsonUrl(Uri url, CancellationToken token = default)
        {
            _client.DefaultRequestHeaders.Add("Origin", "https://speedhive.mylaps.com");
            _client.DefaultRequestHeaders.Add("Referer", "https://speedhive.mylaps.com/");
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:133.0)");
            using var response = await _client.GetAsync(url, token);

            if (response.IsSuccessStatusCode)
            {
                var reader = new StreamReader(await response.Content.ReadAsStreamAsync(token), Encoding.UTF8);
                return await ParseJsonAsync(reader, token);
            }

            throw new HttpRequestException($"Unable to get race results from url: {url}, response: {response}, responseCode: {response.StatusCode}");
        }

        public static async Task<IEnumerable<RaceResultRecord>> ParseJsonAsync(StreamReader reader, CancellationToken token = default)
        {
            var results = JsonSerializer.Deserialize<RaceResultResponse>(await reader.ReadToEndAsync(token));

            if (results == null)
            {
                throw new DataException("unable to parse given json stream!");
            }

            return results.rows;
        }

        public static Uri GetApiJsonUrlFromUiUrl(Uri uiUrl)
        {
            var path = uiUrl.PathAndQuery.Split('?')[0].Split('/');
            if (path.Length < 2)
            {
                throw new HttpRequestException($"Unable to get api url from UI url: {uiUrl}");
            }

            // /sessions/8939619?one=two
            var sessionId = path[2];

            return new Uri(string.Format(JsonBaseUrl, ApiVersion, sessionId));
        }

        public static Uri GetApiJsonUrlFromSessionId(string sessionId)
        {
            return new Uri(string.Format(JsonBaseUrl, ApiVersion, sessionId));
        }
    }
}