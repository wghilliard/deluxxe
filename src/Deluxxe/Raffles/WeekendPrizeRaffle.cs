using System.Diagnostics;
using Deluxxe.ModelsV3;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class WeekendPrizeRaffle(ILogger<WeekendPrizeRaffle> logger, ActivitySource activitySource, StickerManager stickerManager) : PrizeRaffle<WeekendPrizeDescription>(logger, activitySource, stickerManager);