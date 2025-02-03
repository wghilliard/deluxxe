using System.Diagnostics;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class RaffleService(ActivitySource activitySource, StickerProviderUriResolver stickerProvider, ILogger<RaffleService> logger, ILogger<PrizeRaffle> prizeRaffleLogger)
{
    public async Task<DrawingResult> ExecuteRaffleAsync(
        RaffleExecutionConfiguration raffleExecutionConfiguration,
        IList<PrizeDescription> prizeDescriptions,
        IList<Driver> drivers,
        IList<PrizeWinner> previousWinners,
        PrizeLimitChecker prizeLimitChecker,
        Func<int, ResourceIdBuilder> resourceIdBuilder
    )
    {
        var stickerManager = await stickerProvider.GetStickerManager(raffleExecutionConfiguration.StickerMapUri);

        logger.LogInformation("starting drawing rounds");
        using var activity = activitySource.StartActivity("starting drawing rounds");
        var randomSeed = DateTimeOffset.UtcNow.Millisecond;
        var random = new Random(randomSeed);

        var scopedPreviousWinners = new List<PrizeWinner>(previousWinners);
        var scopedPrizeDescriptions = new List<PrizeDescription>(prizeDescriptions);
        var results = new List<DrawingRoundResult>();
        for (var round = 0; round < raffleExecutionConfiguration.MaxRounds; round++)
        {
            using var roundActivity = activitySource.StartActivity("drawing-round");
            if (scopedPrizeDescriptions.Count == 0)
            {
                logger.LogInformation("awarded all prizes!");
                break;
            }

            var raffle = new PrizeRaffle(prizeRaffleLogger, activitySource, stickerManager, prizeLimitChecker, random);

            var scopedResourceIdBuilder = resourceIdBuilder(round);

            var drawingResult = raffle.DrawPrizes(scopedPrizeDescriptions, drivers, scopedPreviousWinners, new DrawingConfiguration
                {
                    DrawingType = raffleExecutionConfiguration.DrawingType,
                    Season = raffleExecutionConfiguration.Season,
                    ResourceIdBuilder = scopedResourceIdBuilder,
                },
                round);

            logger.LogInformation($"sat {drawingResult.winners.Count} won");
            logger.LogInformation($"sat {drawingResult.notAwarded.Count} not-awarded");

            results.Add(drawingResult);
            scopedPrizeDescriptions = [..drawingResult.notAwarded];
            scopedPreviousWinners.AddRange(drawingResult.winners);

            if (drawingResult.winners.Count == 0 && raffleExecutionConfiguration.ClearHistoryIfNoCandidates)
            {
                roundActivity?.AddEvent(new ActivityEvent("clearing-previous-winners"));
                scopedPreviousWinners.Clear();
            }
        }

        return new DrawingResult
        {
            drawingType = raffleExecutionConfiguration.DrawingType,
            randomSeed = randomSeed,
            winners = results.SelectMany(result => result.winners).ToList(),
            notAwarded = scopedPrizeDescriptions
        };
    }
}