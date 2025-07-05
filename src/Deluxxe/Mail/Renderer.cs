using System.Globalization;
using Deluxxe.Mail.Messages;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Deluxxe.Mail;

public class Renderer(HtmlRenderer htmlRenderer)
{
    public async Task<string> RenderAnnouncement(RaffleResult raffleResult, IDictionary<string, string> sponsorRepresentationTable)
    {
        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "raffleResult", raffleResult },
                { "sponsorRepresentationTable", sponsorRepresentationTable },
                { "title", $"{raffleResult.season} PRO3 Raffle Prize Winners - {PrettyEventName(raffleResult.name)}" }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<AnnouncementMessage>(parameters);

            return output.ToHtmlString();
        });
    }

    public Task<string> RenderBimmerworld(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap)
    {
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.Bimmerworld,
            "Hello Hank, below are the Bimmerworld Gift Card winners!");
    }

    public Task<string> RenderAaf(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap)
    {
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.AAF,
            "Hello Sam, Hank, below are the AAF Gift Card winners!");
    }

    public Task<string> RenderRoR(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap)
    {
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.RoR,
            "Hello Annie, Gamma, below are the RoR Gift Card / Gas Card winners!");
    }

    public async Task<Dictionary<string, string>> RenderDriverToyo(RaffleResult raffleResult)
    {
        var output = new Dictionary<string, string>();
        var winners = raffleResult.drawings.SelectMany(drawing => drawing.winners)
            .Where(winner => winner.prizeDescription.sponsorName == SponsorConstants.ToyoTires)
            .ToList();

        foreach (var winner in winners)
        {
            var shortName = $"{string.Concat(winner.candidate.name.Split(' ').Select(word => word[0])).ToLower()}-{winner.candidate.carNumber}";
            output.Add(shortName,
                await htmlRenderer.Dispatcher.InvokeAsync(async () =>
                {
                    var dictionary = new Dictionary<string, object?>
                    {
                        { "winner", winner },
                        { "title", $"{raffleResult.season} PRO3 Raffle Prize Winners - {PrettyEventName(raffleResult.name)} - {SponsorConstants.ToyoTires}" },
                    };

                    var parameters = ParameterView.FromDictionary(dictionary);
                    var html = await htmlRenderer.RenderComponentAsync<DriverToyoMessage>(parameters);

                    return html.ToHtmlString();
                }));
        }

        return output;
    }

    private async Task<string> RenderSponsor(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap, string sponsorName, string content)
    {
        var winners = raffleResult.drawings.SelectMany(drawing => drawing.winners)
            .Where(winner => winner.prizeDescription.sponsorName == sponsorName)
            .ToList();

        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "winners", winners },
                { "title", $"{raffleResult.season} PRO3 Raffle Prize Winners - {PrettyEventName(raffleResult.name)} - {sponsorName}" },
                { "emailAddressMap", emailAddressMap },
                { "content", content }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<SponsorMessage>(parameters);

            return output.ToHtmlString();
        });
    }

    private static string PrettyEventName(string eventName)
    {
        return string.Join(' ', eventName.Split("-").Select(word => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word)));
    }
}