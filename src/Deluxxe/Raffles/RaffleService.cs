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

        Action<PrizeDescription[]> shuffle;
        if (raffleExecutionConfiguration.RandomShuffleSeed == 0)
        {
            shuffle = prizes => RandomNumberGenerator.Shuffle<PrizeDescription>(prizes);
        }
        else
        {
            activity?.AddTag("randomShuffleSeed", raffleExecutionConfiguration.RandomShuffleSeed);
            shuffle = prizes => new Random(raffleExecutionConfiguration.RandomShuffleSeed).Shuffle(prizes);
        }

        var prizeDescriptionsArray = prizeDescriptions.ToArray();
        shuffle(prizeDescriptionsArray);

        Func<int, int> getNextRandomInt;
        if (raffleExecutionConfiguration.RandomDrawingSeed == 0)
        {
            activity?.AddTag("randomDrawingSeed", "none");
            getNextRandomInt = RandomNumberGenerator.GetInt32;
        }
        else
        {
            activity?.AddTag("randomDrawingSeed", raffleExecutionConfiguration.RandomDrawingSeed);
            getNextRandomInt = new Random(raffleExecutionConfiguration.RandomDrawingSeed).Next;
        }

        var scopedPreviousWinners = raffleExecutionConfiguration.FilterDriversWithWinningHistory ? new List<PrizeWinner>(previousWinners) : new List<PrizeWinner>();
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

            var raffle = new PrizeRaffle(prizeRaffleLogger, activitySource, stickerManager, prizeLimitChecker, getNextRandomInt);

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
            logger.LogInformation("{winnersCount} won", drawingResult.winners.Count);
            logger.LogInformation("{notAwardedCount} not-awarded", drawingResult.notAwarded.Count);

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