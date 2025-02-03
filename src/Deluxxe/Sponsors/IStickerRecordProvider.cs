namespace Deluxxe.Sponsors;

public interface IStickerRecordProvider
{
    public Task<StickerParseResult?> Get(Uri uri);
}