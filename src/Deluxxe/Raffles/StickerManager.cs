namespace Deluxxe.Raffles;

public class StickerManager(IReadOnlyDictionary<string, string> driverToCarMap, IDictionary<string, IDictionary<string, bool>> carToStickerMap)
{
    public bool DriverHasSticker(string driverName, string sponsorName)
    {
        if (!driverToCarMap.TryGetValue(driverName, out var car))
        {
            throw new ArgumentException($"driver-to-car mapping for {driverName} does not exist");
        }

        if (!carToStickerMap.TryGetValue(car, out var carStickers))
        {
            // the car-sticker mapping is missing for this car, need to update
            return false;
        }

        if (!carStickers.TryGetValue(sponsorName, out var carHasSticker))
        {
            // the car-sticker mapping is missing this sponsor
            return false;
        }

        return carHasSticker;
    }
}