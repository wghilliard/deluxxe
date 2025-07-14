using System.Diagnostics;
using System.Globalization;
using Deluxxe.Mail.Collateral;
using Deluxxe.Mail.Messages;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Deluxxe.Mail;

public class Renderer(ActivitySource activitySource, HtmlRenderer htmlRenderer, OperatorConfiguration operatorConfiguration)
{
    public async Task<string> RenderAnnouncement(RaffleResult raffleResult, IDictionary<string, string> sponsorRepresentationTable)
    {
        using var activity = activitySource.StartActivity(nameof(RenderAnnouncement));
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
        using var activity = activitySource.StartActivity(nameof(RenderBimmerworld));
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.Bimmerworld,
            "Hello Hank, below are the Bimmerworld Gift Card winners!");
    }

    public Task<string> RenderAaf(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap)
    {
        using var activity = activitySource.StartActivity(nameof(RenderAaf));
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.AAF,
            "Hello Sam, Hank, below are the AAF Gift Card winners!");
    }

    public Task<string> RenderRoR(RaffleResult raffleResult, Dictionary<string, string> emailAddressMap)
    {
        using var activity = activitySource.StartActivity(nameof(RenderRoR));
        return RenderSponsor(raffleResult,
            emailAddressMap,
            SponsorConstants.RoR,
            "Hello Annie, Gama, below are the RoR Gift Card / Gas Card winners! I will be responsible for distributing the Gas Cards!");
    }

    public async Task<Dictionary<string, string>> RenderDriverToyo(RaffleResult raffleResult)
    {
        using var activity = activitySource.StartActivity(nameof(RenderDriverToyo));
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

    public async Task<Dictionary<string, string>> RenderToyoAwardCollateral(RaffleResult raffleResult)
    {
        using var activity = activitySource.StartActivity(nameof(RenderToyoAwardCollateral));
        var output = new Dictionary<string, string>();
        var winners = raffleResult.drawings.SelectMany(drawing => { return drawing.winners.Select(winner => (winner, drawing)); })
            .Where(tuple => tuple.winner.prizeDescription.sponsorName == SponsorConstants.ToyoTires)
            .ToList();

        foreach (var (winner, drawing) in winners)
        {
            var shortName = $"{string.Concat(winner.candidate.name.Split(' ').Select(word => word[0])).ToLower()}-{winner.candidate.carNumber}";
            output.Add(shortName, await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var dictionary = new Dictionary<string, object?>
                {
                    { "winner", winner },
                    { "eventName", PrettyEventName(raffleResult.name) },
                    { "eventDate", drawing.startTime },
                    { "trackName", raffleResult.trackName },
                    { "numCars", drawing.eligibleCandidatesCount },
                    { "officialsInitials", string.Concat(operatorConfiguration.name.Split(' ').Select(a => a.First())) },
                    { "officialsName", operatorConfiguration.name },
                };

                var parameters = ParameterView.FromDictionary(dictionary);
                var html = await htmlRenderer.RenderComponentAsync<ToyoAward>(parameters);

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