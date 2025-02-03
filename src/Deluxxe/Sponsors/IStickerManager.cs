namespace Deluxxe.Sponsors;

public interface IStickerManager
{
    public StickerStatus DriverHasSticker(string carNumber, string sponsorName);
}