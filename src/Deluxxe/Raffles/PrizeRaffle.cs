using System.Diagnostics;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class PrizeRaffle(ILogger<PrizeRaffle> logger, ActivitySource activitySource, IStickerManager stickerManager, PrizeLimitChecker prizeLimitChecker, Random random)
{
    public DrawingRoundResult DrawPrizes(
        IList<PrizeDescription> descriptions,
        IList<Driver> weekendRaceResults,
        IList<PrizeWinner> previousWinners,
        DrawingConfiguration drawingConfig,
        int round)
    {
        var prizeWinners = new List<PrizeWinner>();
        var notAwarded = new List<PrizeDescription>();

        var allPreviousWinners = previousWinners.ToList();
        logger.LogInformation("start drawing prizes");
        using var activity = activitySource.StartActivity("drawing-prizes");
        activity?.AddTag("round", round);
        foreach (var description in descriptions)
        {
            logger.LogInformation("start drawing for [description={}]", description);

            var winner = DrawPrize(description, weekendRaceResults, allPreviousWinners, drawingConfig);
            if (winner != null)
            {
                logger.LogInformation("winner found for [description={}]", description);
                allPreviousWinners.Add(winner);
                prizeWinners.Add(winner);
            }
            else
            {
                logger.LogInformation("no winner found for [description={}]", description);
                notAwarded.Add(description);
            }
        }

        activity?.AddTag("awarded", prizeWinners.Count);
        activity?.AddTag("notAwarded", notAwarded.Count);

        return new DrawingRoundResult()
        {
            winners = prizeWinners,
            notAwarded = notAwarded
        };
    }

    public PrizeWinner? DrawPrize(PrizeDescription description, IList<Driver> weekendRaceResults, IList<PrizeWinner> previousWinners, DrawingConfiguration drawingConfig)
    {
        using var activity = activitySource.StartActivity("processing-candidates");

        var eligibleCandidates = weekendRaceResults.Where(raceResult =>
            {
                using var candidateActivity = activitySource.StartActivity("processing-candidate");

                candidateActivity?.AddTag("sponsor", description.sponsorName);
                candidateActivity?.AddTag("driveName", raceResult.name);

                var stickerStatus = stickerManager.DriverHasSticker(raceResult.carNumber, description.sponsorName);
                candidateActivity?.AddTag("stickerStatus", stickerStatus);

                if (stickerStatus != StickerStatus.CarHasSticker)
                {
                    logger.LogTrace("no sticker for this sponsor [driver={}] [description={}]", raceResult.name, description);

                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.StickerNotPresent);
                    return false;
                }

                if (previousWinners.Any(winner => winner.driver.name == raceResult.name))
                {
                    logger.LogTrace("this driver has previously won [driver={}] [description={}]", raceResult.name, description);
                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.PreviouslyWonThisSession);
                    return false;
                }

                if (!prizeLimitChecker.IsBelowLimit(description, raceResult))
                {
                    logger.LogTrace("this driver has reached their seasonal limit [driver={}] [description={}]", raceResult.name, description);
                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.SeasonalLimitExceeded);
                    return false;
                }

                candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.None);
                return true;
            }
        ).ToList();

        logger.LogTrace("eligible candidates {}", eligibleCandidates.Count);
        activity?.AddTag("eligible-candidates", eligibleCandidates.Count);
        activity?.AddTag("ineligible-candidates", weekendRaceResults.Count - eligibleCandidates.Count);

        if (eligibleCandidates.Count == 0)
        {
            // No candidates available, track the prize as not-awarded
            logger.LogTrace("no eligible candidates");
            activity?.AddTag("status-message", "no eligible candidates");
            activity?.SetStatus(ActivityStatusCode.Error);
            return null;
        }

        var winnerIndex = random.Next(eligibleCandidates.Count);
        var winner = eligibleCandidates[winnerIndex];

        logger.LogTrace("winner found [name={}]", winner.name);
        activity?.AddTag("prize-winner", winner.name);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return new PrizeWinner
        {
            driver = winner,
            prizeDescription = description,
            resourceId = drawingConfig.ResourceIdBuilder.Copy().WithPrize(description.sponsorName, description.sku).Build(),
        };
    }

    private static bool HasWonToyo(string driverName, string sku, IList<PrizeWinner> previousWinners)
    {
        return previousWinners
            .Where(winner => winner.driver.name == driverName)
            .Any(winner => winner.prizeDescription.sponsorName == SponsorConstants.ToyoTires
                           && winner.prizeDescription.sku == sku);
    }
}