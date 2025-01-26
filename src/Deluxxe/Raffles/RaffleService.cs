using System.Diagnostics;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class RaffleService(ActivitySource activitySource, StickerProviderUriResolver stickerProvider, IServiceProvider serviceProvider, ILogger<RaffleService> logger)
{
    public async Task<DrawingResult> ExecuteRaffleAsync(
        RaffleConfiguration raffleConfiguration,
        IList<PrizeDescription> prizeDescriptions,
        IList<Driver> drivers,
        IList<PrizeWinner> previousWinners,
        Func<int, ResourceIdBuilder> resourceIdBuilder
    )
    {
        var raffleLogger = serviceProvider.GetRequiredService<ILogger<PrizeRaffle>>();
        var stickerManager = await stickerProvider.GetStickerManager(raffleConfiguration.StickerMapUri);

        logger.LogInformation("starting drawing rounds");
        using var activity = activitySource.StartActivity("starting drawing rounds");
        var randomSeed = DateTimeOffset.UtcNow.Millisecond;
        var random = new Random(randomSeed);

        var scopedPrizeDescriptions = prizeDescriptions;
        var results = new List<DrawingRoundResult>();
        for (var round = 0; round < raffleConfiguration.MaxRounds; round++)
        {
            if (scopedPrizeDescriptions.Count == 0)
            {
                logger.LogInformation("awarded all prizes!");
                break;
            }

            var raffle = new PrizeRaffle(raffleLogger, activitySource, stickerManager, random);

            var scopedResourceIdBuilder = resourceIdBuilder(round);

            var drawingResult = raffle.DrawPrizes(scopedPrizeDescriptions, drivers, previousWinners, new DrawingConfiguration
                {
                    DrawingType = raffleConfiguration.DrawingType,
                    Season = raffleConfiguration.Season,
                    ResourceIdBuilder = scopedResourceIdBuilder,
                },
                round);

            logger.LogInformation($"sat {drawingResult.winners.Count} won");
            logger.LogInformation($"sat {drawingResult.notAwarded.Count} not-awarded");

            results.Add(drawingResult);
            scopedPrizeDescriptions = drawingResult.notAwarded;
        }

        return new DrawingResult()
        {
            drawingType = raffleConfiguration.DrawingType,
            randomSeed = randomSeed,
            winners = results.SelectMany(result => result.winners).ToList(),
            notAwarded = scopedPrizeDescriptions
        };
    }
}