using System.Diagnostics;
using Deluxxe.ModelsV3;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class PrizeRaffle<T>(ILogger<PrizeRaffle<T>> logger, ActivitySource activitySource, IStickerManager stickerManager)
    where T : PrizeDescription
{
    public (IList<PrizeWinner<T>> winners, IList<T> notAwarded) DrawPrizes(
        IList<T> descriptions,
        IList<RaceResult> weekendRaceResults,
        IList<PrizeWinner<T>> previousWinners)
    {
        var prizeWinners = new List<PrizeWinner<T>>();
        var notAwarded = new List<T>();

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

        return (prizeWinners, notAwarded);
    }

    public PrizeWinner<T>? DrawPrize(T description, IList<RaceResult> weekendRaceResults, IList<PrizeWinner<T>> previousWinners)
    {
        using var activity = activitySource.StartActivity("processing-candidates");

        var eligibleCandidates = weekendRaceResults.Where(raceResult =>
            {
                using var candidateActivity = activitySource.StartActivity("processing-candidate");

                candidateActivity?.AddTag("sponsor", description.SponsorName);
                candidateActivity?.AddTag("driveName", raceResult.Driver.Name);

                var stickerStatus = stickerManager.DriverHasSticker(raceResult.Driver.CarNumber, description.SponsorName);
                candidateActivity?.AddTag("stickerStatus", stickerStatus);
                
                if (stickerStatus != StickerStatus.CarHasSticker)
                {
                    logger.LogInformation("no sticker for this sponsor [driver={}] [description={}]", raceResult.Driver.Name, description);

                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.StickerNotPresent);
                    return false;
                }

                if (previousWinners.Any(winner => winner.Driver.Name == raceResult.Driver.Name))
                {
                    logger.LogInformation("this driver has previously won [driver={}] [description={}]", raceResult.Driver.Name, description);
                    candidateActivity?.AddTag("ineligibility-reason", IneligibilityReason.PreviouslyWonThisSession);
                    return false;
                }

                if (description.SponsorName == Constants.ToyoTiresSponsorName && HasWonToyo(raceResult.Driver.Name, previousWinners))
                {
                    logger.LogInformation("this driver has previously won Toyo Tires[driver={}] [description={}]", raceResult.Driver.Name, description);
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

        var random = new Random();
        var winnerIndex = random.Next(eligibleCandidates.Count);
        var winner = eligibleCandidates[winnerIndex];

        logger.LogInformation("winner found [name={}]", winner.Driver.Name);
        activity?.AddTag("prize-winner", winner.Driver.Name);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return new PrizeWinner<T>()
        {
            Driver = winner.Driver,
            PrizeDescription = description
        };
    }

    private static bool HasWonToyo(string driverName, IList<PrizeWinner<T>> previousWinners)
    {
        // Implement logic to check if the driver has won a Toyo prize
        return previousWinners
            .Where(winner => winner.Driver.Name == driverName)
            .Any(winner => winner.PrizeDescription.SponsorName == Constants.ToyoTiresSponsorName);
    }
}