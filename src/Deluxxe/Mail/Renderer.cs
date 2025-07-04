using Deluxxe.Raffles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Deluxxe.Mail;

public class Renderer(HtmlRenderer htmlRenderer)
{
    public async Task<string> Render(RaffleResult raffleResult,  IDictionary<string, string> sponsorRepresentationTable)
    {
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "raffleResult", raffleResult },
                { "sponsorRepresentationTable", sponsorRepresentationTable }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<AnnouncementMessage>(parameters);

            return output.ToHtmlString();
        });

        return html;
    }
}