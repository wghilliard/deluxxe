using Deluxxe.ModelsV3;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class PrizeRaffle<T>(ILogger<PrizeRaffle<T>> logger, StickerManager stickerManager)
    where T : PrizeDescription
{
    public (IList<PrizeWinner<T>> winners, IList<T> notAwarded) DrawPrizes(
        IReadOnlyList<T> descriptions, 
        IReadOnlyList<RaceResult> weekendRaceResults,
        IReadOnlyList<PrizeWinner<T>> previousWinners)
    {
        var prizeWinners = new List<PrizeWinner<T>>();
        var notAwarded = new List<T>();

        var allPreviousWinners = previousWinners.ToList();
        logger.LogInformation("start drawing prizes");
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

        return (prizeWinners, notAwarded);
    }

    public PrizeWinner<T>? DrawPrize(T description, IReadOnlyList<RaceResult> weekendRaceResults, IReadOnlyList<PrizeWinner<T>> previousWinners)
    {
        var eligibleCandidates = weekendRaceResults.Where(raceResult =>
            {
                if (!stickerManager.DriverHasSticker(raceResult.Driver.Name, description.SponsorName))
                {
                    logger.LogInformation("no sticker for this sponsor [driver={}] [description={}]", raceResult.Driver.Name, description);
                    return false;
                }

                if (previousWinners.Any(winner => winner.Driver.Name == raceResult.Driver.Name))
                {
                    logger.LogInformation("this driver has previously won [driver={}] [description={}]", raceResult.Driver.Name, description);
                    return false;
                }

                if (description.SponsorName == Constants.ToyoTiresSponsorName && HasWonToyo(raceResult.Driver.Name, previousWinners))
                {
                    logger.LogInformation("this driver has previously won Toyo Tires[driver={}] [description={}]", raceResult.Driver.Name, description);
                }

                // && !awarded.Any(award => award.Winner.DriverName == drive.DriverName && award.Prize.SponsorName == description.SponsorName)

                return true;
            }
        ).ToList();

        logger.LogInformation("eligible candidates {}", eligibleCandidates.Count);

        if (eligibleCandidates.Count == 0)
        {
            // No candidates available, track the prize as not-awarded
            logger.LogInformation("no eligible candidates");

            return null;
        }

        var random = new Random();
        var winnerIndex = random.Next(eligibleCandidates.Count);
        var winner = eligibleCandidates[winnerIndex];

        logger.LogInformation("winner found [name={}]", winner.Driver.Name);

        return new PrizeWinner<T>()
        {
            Driver = winner.Driver,
            PrizeDescription = description
        };
    }

    private static bool HasWonToyo(string driverName, IReadOnlyList<PrizeWinner<T>> previousWinners)
    {
        // Implement logic to check if the driver has won a Toyo prize
        return previousWinners
            .Where(winner => winner.Driver.Name == driverName)
            .Any(winner => winner.PrizeDescription.SponsorName == Constants.ToyoTiresSponsorName);
    }
}