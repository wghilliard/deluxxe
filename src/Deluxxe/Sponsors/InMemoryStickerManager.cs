namespace Deluxxe.Sponsors;

public class InMemoryStickerManager(IDictionary<string, IDictionary<string, bool>> carToStickerMap): IStickerManager
{
    public StickerStatus DriverHasSticker(string carNumber, string sponsorName)
    {
        if (!carToStickerMap.TryGetValue(carNumber, out var carStickers))
        {
            return StickerStatus.StickerMapMissingForCar;
        }

        if (!carStickers.TryGetValue(sponsorName, out var carHasSticker))
        {
            return StickerStatus.StickerValueMissingForCar;
        }

        return carHasSticker ? StickerStatus.CarHasSticker : StickerStatus.CarDoesNotHaveSticker;
    }
}