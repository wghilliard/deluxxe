namespace Deluxxe.Raffles;

public enum StickerStatus
{
    CarHasSticker,
    CarDoesNotHaveSticker,
    DriverNotAssignedToCar,
    StickerMapMissingForCar, // the car-sticker mapping is missing for this car, need to update
    StickerValueMissingForCar // the car-sticker mapping is missing this sponsor
}