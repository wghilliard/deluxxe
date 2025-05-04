using System.Diagnostics;
using System.Security.Cryptography;
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
        var stickerManager = await stickerProvider.GetStickerManager(raffleExecutionConfiguration.StickerMapUri, raffleExecutionConfiguration.StickerMapSchemaVersion);

        logger.LogInformation("starting drawing rounds");
        using var activity = activitySource.StartActivity("starting drawing rounds");
        // var randomShuffleSeed = raffleExecutionConfiguration.RandomSeed == 0 ? RandomNumberGenerator.GetInt32(int.MaxValue) : raffleExecutionConfiguration.RandomSeed;
        // activity?.AddTag("randomShuffleSeed", randomShuffleSeed.ToString());
        // var randomShuffle = new Random(randomShuffleSeed);

        var prizeDescriptionsArray = prizeDescriptions.ToArray();
        RandomNumberGenerator.Shuffle<PrizeDescription>(prizeDescriptionsArray);

        // var randomDrawingSeed = raffleExecutionConfiguration.RandomSeed == 0 ? RandomNumberGenerator.GetInt32(int.MaxValue) : raffleExecutionConfiguration.RandomSeed;
        // activity?.AddTag("randomDrawingSeed", randomDrawingSeed.ToString());
        // var randomDrawing = new Random(randomDrawingSeed);

        var scopedPreviousWinners = raffleExecutionConfiguration.UseWinningHistory ? new List<PrizeWinner>(previousWinners) : new List<PrizeWinner>();
        var scopedPrizeDescriptions = new List<PrizeDescription>(prizeDescriptionsArray);
        var results = new List<DrawingRoundResult>();
        for (var round = 0; round < raffleExecutionConfiguration.MaxRounds; round++)
        {
            using var roundActivity = activitySource.StartActivity("drawing-round");
            roundActivity?.SetTag("round", round);
            if (scopedPrizeDescriptions.Count == 0)
            {
                logger.LogInformation("awarded all prizes!");
                break;
            }

            var raffle = new PrizeRaffle(prizeRaffleLogger, activitySource, stickerManager, prizeLimitChecker);

            var scopedResourceIdBuilder = resourceIdBuilder(round);

            var drawingResult = raffle.DrawPrizes(scopedPrizeDescriptions, drivers, scopedPreviousWinners, new DrawingConfiguration
                {
                    DrawingType = raffleExecutionConfiguration.DrawingType,
                    Season = raffleExecutionConfiguration.Season,
                    ResourceIdBuilder = scopedResourceIdBuilder,
                },
                round);

            roundActivity?.SetTag("awarded prizes", drawingResult.winners.Count);
            roundActivity?.SetTag("not-awarded prizes", drawingResult.notAwarded.Count);
            logger.LogInformation($"{drawingResult.winners.Count} won");
            logger.LogInformation($"{drawingResult.notAwarded.Count} not-awarded");

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
            winners = results.SelectMany(result => result.winners).ToList(),
            notAwarded = scopedPrizeDescriptions
        };
    }
}