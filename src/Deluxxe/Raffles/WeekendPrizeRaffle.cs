using Deluxxe.ModelsV3;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class WeekendPrizeRaffle(ILogger<WeekendPrizeRaffle> logger, StickerManager stickerManager) : PrizeRaffle<WeekendPrizeDescription>(logger, stickerManager);