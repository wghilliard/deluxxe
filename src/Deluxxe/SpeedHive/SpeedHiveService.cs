
using System.Net.Http.Json;

namespace Deluxxe.SpeedHive;

public class SpeedHiveService(HttpClient httpClient)
{
    public async Task<List<SpeedHiveEvent>?> GetEventsAsync(string mylapsAccountId, CancellationToken token = default)
    {
        var url = $"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/accounts/{mylapsAccountId}/events?sportCategory=Motorized&count=100";
        return await httpClient.GetFromJsonAsync<List<SpeedHiveEvent>>(url, token);
    }

    public async Task<SpeedHiveEventDetails?> GetEventDetailsAsync(int eventId, CancellationToken token = default)
    {
        var url = $"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/events/{eventId}?sessions=true";
        return await httpClient.GetFromJsonAsync<SpeedHiveEventDetails>(url, token);
    }
}
