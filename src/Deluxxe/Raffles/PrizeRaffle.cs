using System.Diagnostics;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class PrizeRaffle(ILogger<PrizeRaffle> logger, ActivitySource activitySource, IStickerManager stickerManager, int randomSeed)
{
    private readonly Random _random = new(randomSeed);

    public DrawingResult DrawPrizes(
        IList<PrizeDescription> descriptions,
        IList<Driver> weekendRaceResults,
        IList<PrizeWinner> previousWinners,
        DrawingType drawingType)
    {
        var prizeWinners = new List<PrizeWinner>();
        var notAwarded = new List<PrizeDescription>();

        var allPreviousWinners = previousWinners.ToList();
        logger.LogInformation("start drawing prizes");
        using var activity = activitySource.StartActivity("drawing-prizes");
        foreach (var description in descriptions)
        {
            logger.LogInformation("start drawing for [description={}]", description);

            var winner = DrawPrize(description, weekendRaceResults, allPreviousWinners);
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

        return new DrawingResult()
        {
            winners = prizeWinners,
            notAwarded = notAwarded,
            drawingType = drawingType,
            randomSeed = randomSeed
        };
    }

    public PrizeWinner? DrawPrize(PrizeDescription description, IList<Driver> weekendRaceResults, IList<PrizeWinner> previousWinners)
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
                    logger.LogInformation("no sticker for this sponsor [driver={}] [description={}]", raceResult.name, description);

                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.StickerNotPresent);
                    return false;
                }

                if (previousWinners.Any(winner => winner.driver.name == raceResult.name))
                {
                    logger.LogInformation("this driver has previously won [driver={}] [description={}]", raceResult.name, description);
                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.PreviouslyWonThisSession);
                    return false;
                }

                if (description.sponsorName == Constants.ToyoTiresSponsorName && HasWonToyo(raceResult.name, previousWinners))
                {
                    logger.LogInformation("this driver has previously won Toyo Tires[driver={}] [description={}]", raceResult.name, description);
                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.PreviouslyWonThisSeason);
                    return false;
                }

                candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.None);
                return true;
            }
        ).ToList();

        logger.LogInformation("eligible candidates {}", eligibleCandidates.Count);
        activity?.AddTag("eligible-candidates", eligibleCandidates.Count);
        activity?.AddTag("ineligible-candidates", weekendRaceResults.Count - eligibleCandidates.Count);

        if (eligibleCandidates.Count == 0)
        {
            // No candidates available, track the prize as not-awarded
            logger.LogInformation("no eligible candidates");
            activity?.AddTag("status-message", "no eligible candidates");
            activity?.SetStatus(ActivityStatusCode.Error);
            return null;
        }

        var winnerIndex = _random.Next(eligibleCandidates.Count);
        var winner = eligibleCandidates[winnerIndex];

        logger.LogInformation("winner found [name={}]", winner.name);
        activity?.AddTag("prize-winner", winner.name);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return new PrizeWinner()
        {
            driver = winner,
            prizeDescription = description
        };
    }

    private static bool HasWonToyo(string driverName, IList<PrizeWinner> previousWinners)
    {
        // Implement logic to check if the driver has won a Toyo prize
        return previousWinners
            .Where(winner => winner.driver.name == driverName)
            .Any(winner => winner.prizeDescription.sponsorName == Constants.ToyoTiresSponsorName);
    }
}