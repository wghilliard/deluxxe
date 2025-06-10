using Microsoft.Extensions.Logging;

namespace Deluxxe.Sponsors;

public class InMemoryStickerManager(ILogger<InMemoryStickerManager> logger, StickerParseResult parseResult, bool allowRentersToWin) : IStickerManager
{
    public StickerStatus DriverHasSticker(string carNumber, string sponsorName)
    {
        if (!parseResult.carToStickerMapping.TryGetValue(carNumber, out var carStickers))
        {
            return StickerStatus.StickerMapMissingForCar;
        }

        if (!carStickers.TryGetValue(sponsorName.ToLower(), out var carHasSticker))
        {
            return StickerStatus.StickerValueMissingForCar;
        }

        return carHasSticker ? StickerStatus.CarHasSticker : StickerStatus.CarDoesNotHaveSticker;
    }

    public string GetCandidateNameForCar(string carNumber, string driverName)
    {
        if (allowRentersToWin)
        {
            return driverName;
        }

        if (parseResult.carRentalMap.TryGetValue(carNumber, out var carOwnerName))
        {
            return carOwnerName;
        }

        return driverName;
    }

    public StickerParseResult GetParseResult() => parseResult;
}