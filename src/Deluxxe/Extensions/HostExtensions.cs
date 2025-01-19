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

        services.AddTransient<PrizeRaffle>();
        return services;
    }
}