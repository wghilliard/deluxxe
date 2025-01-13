namespace Deluxxe.Raffles;

public class StickerManager(IReadOnlyDictionary<string, string> driverToCarMap, IDictionary<string, IDictionary<string, bool>> carToStickerMap)
{
    public StickerStatus DriverHasSticker(string driverName, string sponsorName)
    {
        if (!driverToCarMap.TryGetValue(driverName, out var car))
        {
            return StickerStatus.DriverNotAssignedToCar;
        }

        if (!carToStickerMap.TryGetValue(car, out var carStickers))
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