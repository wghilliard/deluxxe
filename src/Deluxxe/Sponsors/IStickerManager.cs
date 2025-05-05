namespace Deluxxe.Sponsors;

public interface IStickerManager
{
    public StickerStatus DriverHasSticker(string carNumber, string sponsorName);

    public string GetCandidateNameForCar(string carNumber, string driverName);

    public StickerParseResult GetParseResult();
}