using System.Diagnostics;
using Deluxxe.Raffles;

namespace Deluxxe.Sponsors;

public class StickerProviderUriResolver(ActivitySource activitySource, IEnumerable<IStickerRecordProvider> recordProviders, RaffleConfiguration raffleConfiguration)
{
    private async Task<StickerParseResult> Get(Uri uri, string schemaVersion)
    {
        using var activity = activitySource.StartActivity("resolve-sticker-map-uri");
        foreach (var provider in recordProviders)
        {
            var result = await provider.Get(uri, schemaVersion);
            if (!result.IsEmpty())
            {
                return result;
            }
        }

        return default;
    }

    public async Task<IStickerManager> GetStickerManager(Uri uri, string schemaVersion)
    {
        var parseResult = await Get(uri, schemaVersion);
        if (parseResult.IsEmpty())
        {
            throw new Exception("Unable to resolve sticker map uri");
        }

        return new InMemoryStickerManager(parseResult, raffleConfiguration.allowRentersToWin);
    }
}