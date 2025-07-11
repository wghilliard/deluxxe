using Deluxxe.RaceResults;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.DependencyInjection;

namespace Deluxxe.Extensions;

public static class HostExtensions
{
    public static IServiceCollection AddDeluxxe(this IServiceCollection services)
    {
        services.AddSingleton<IStickerManager, InMemoryStickerManager>();
        services.AddSingleton<IStickerRecordProvider, CsvStickerRecordProvider>();
        services.AddSingleton<StickerProviderUriResolver>();
        services.AddSingleton<RaffleService>();
        services.AddSingleton<RaceResultsService>();
        services.AddSingleton<SpeedHiveClient>();
        services.AddSingleton<PreviousWinnerLoader>();

        services.AddTransient<PrizeRaffle>();
        return services;
    }

    public static IServiceCollection AddDeluxxeJson(this IServiceCollection services)
    {
        services.AddSingleton<IRaffleResultWriter, JsonRaffleResultWriter>();
        services.AddSingleton<IRaffleResultWriter, CsvRaffleResultWriter>();
        services.AddSingleton<IRaffleResultReader, JsonRaffleResultReader>();

        return services;
    }
}