using System.Diagnostics;

namespace Deluxxe.Sponsors;

public class StickerProviderUriResolver(ActivitySource activitySource, IEnumerable<IStickerRecordProvider> recordProviders)
{
    public async Task<StickerParseResult?> Get(Uri uri)
    {
        using var activity = activitySource.StartActivity("resolve-sticker-map-uri");
        foreach (var provider in recordProviders)
        {
            var result = await provider.Get(uri);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public async Task<IStickerManager> GetStickerManager(Uri uri)
    {
        var mapping = (await Get(uri))?.CarToStickerMapping;
        if (mapping == null)
        {
            throw new Exception("Unable to resolve sticker map uri");
        }

        return new InMemoryStickerManager(mapping);
    }
}