namespace Deluxxe.Raffles;

public enum IneligibilityReason
{
    None = 0,
    StickerNotPresent = 1,
    PreviouslyWonThisSession = 2,
    PreviouslyWonThisSeason = 3,
}