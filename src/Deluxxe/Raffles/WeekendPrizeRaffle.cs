using Deluxxe.ModelsV3;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Raffles;

public class WeekendPrizeRaffle
{
    private readonly StickerManager _stickerManager;

    private readonly ILogger<WeekendPrizeRaffle> _logger;

    public WeekendPrizeRaffle(ILogger<WeekendPrizeRaffle> logger, StickerManager stickerManager)
    {
        _logger = logger;
        _stickerManager = stickerManager;
    }

    public (IList<WeekendPrizeWinner> winners, IList<WeekendPrizeDescription> unwarded) DrawPrizes(IReadOnlyList<WeekendPrizeDescription> descriptions,
        IReadOnlyList<RaceResult> weekendRaceResults,
        IReadOnlyList<WeekendPrizeWinner> previousWinners)
    {
        var prizeWinners = new List<WeekendPrizeWinner>();
        var unawarded = new List<WeekendPrizeDescription>();

        var allPreviousWinners = previousWinners.ToList();
        _logger.LogInformation("start drawing prizes");
        foreach (var description in descriptions)
        {
            _logger.LogInformation("start drawing for [description={}]", description);

            var winner = DrawPrize(description, weekendRaceResults, allPreviousWinners);
            if (winner != null)
            {
                _logger.LogInformation("winner found for [description={}]", description);

                allPreviousWinners.Add(winner);
                prizeWinners.Add(winner);
            }
            else
            {
                _logger.LogInformation("no winner found for [description={}]", description);

                unawarded.Add(description);
            }
        }

        return (prizeWinners, unawarded);
    }

    public WeekendPrizeWinner? DrawPrize(WeekendPrizeDescription description,
        IReadOnlyList<RaceResult> weekendRaceResults,
        IReadOnlyList<WeekendPrizeWinner> previousWinners)
    {
        var eligibleCandidates = weekendRaceResults.Where(raceResult =>
            {
                if (!_stickerManager.DriverHasSticker(raceResult.Driver.Name, description.SponsorName))
                {
                    _logger.LogInformation("no sticker for this sponsor [driver={}] [description={}]", raceResult.Driver.Name, description);
                    return false;
                }

                if (previousWinners.Any(winner => winner.Driver.Name == raceResult.Driver.Name))
                {
                    _logger.LogInformation("this driver has previously won [driver={}] [description={}]", raceResult.Driver.Name, description);
                    return false;
                }

                if (description.SponsorName == "Toyo" && HasWonToyo(raceResult.Driver.Name, previousWinners))
                {
                    _logger.LogInformation("this driver has previously won Toyo Tires[driver={}] [description={}]", raceResult.Driver.Name, description);
                }

                // && !awarded.Any(award => award.Winner.DriverName == drive.DriverName && award.Prize.SponsorName == description.SponsorName)

                return true;
            }
        ).ToList();

        _logger.LogInformation("eligible candidates {}", eligibleCandidates.Count);

        if (eligibleCandidates.Count == 0)
        {
            // No candidates available, track the prize as unawarded
            _logger.LogInformation("no eligible candidates");

            return null;
        }

        var random = new Random();
        var winnerIndex = random.Next(eligibleCandidates.Count);
        var winner = eligibleCandidates[winnerIndex];

        _logger.LogInformation("winner found [name={}]", winner.Driver.Name);

        return new WeekendPrizeWinner()
        {
            Driver = winner.Driver,
            PrizeDescription = description
        };
    }

    private static bool HasWonToyo(string driverName, IReadOnlyList<WeekendPrizeWinner> previousWinners)
    {
        // Implement logic to check if the driver has won a Toyo prize
        return previousWinners
            .Where(winner => winner.Driver.Name == driverName)
            .Any(winner => winner.PrizeDescription.SponsorName == "Toyo");
    }
}