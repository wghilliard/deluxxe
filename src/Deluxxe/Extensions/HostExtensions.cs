using Deluxxe.Google;
using Deluxxe.Mail;
using Deluxxe.PDF;
using Deluxxe.RaceResults;
using Deluxxe.RaceResults.SpeedHive;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Deluxxe.Extensions;

public static class HostExtensions
{
    public static IServiceCollection AddDeluxxe(this IServiceCollection services)
    {
        // Deluxxe.Sponsors
        services.AddSingleton<IStickerManager, InMemoryStickerManager>();
        services.AddSingleton<IStickerRecordProvider, CsvStickerRecordProvider>();
        services.AddSingleton<StickerProviderUriResolver>();
        services.AddSingleton<RepresentationCalculator>();

        // Deluxxe.Raffles
        services.AddSingleton<RaffleService>();
        services.AddSingleton<PreviousWinnerLoader>();
        services.AddTransient<PrizeRaffle>();

        // Deluxxe.RaceResults
        services.AddSingleton<RaceResultsService>();
        services.AddSingleton<SpeedHiveClient>();

        // Deluxxe.Mail
        services.AddSingleton<Renderer>();
        services.AddSingleton<HtmlRenderer>();

        // Deluxxe.Google
        services.AddSingleton<GoogleSheetService>();

        // Deluxxe.PDF
        services.AddSingleton<RenderingClient>();
        services.AddSingleton<ProxyClient>();
        services.AddSingleton<IPlaywright>(_ => Playwright.CreateAsync().GetAwaiter().GetResult());

        return services;
    }

    public static IServiceCollection AddDeluxxeJson(this IServiceCollection services)
    {
        // Deluxxe.Raffles
        services.AddSingleton<IRaffleResultWriter, JsonRaffleResultWriter>();
        services.AddSingleton<IRaffleResultWriter, CsvRaffleResultWriter>();
        services.AddSingleton<IRaffleResultReader, JsonRaffleResultReader>();

        return services;
    }
}