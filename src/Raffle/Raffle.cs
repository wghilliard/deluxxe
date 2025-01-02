using Deluxxe.Models;

namespace Deluxxe.Raffle;

public class Raffle
{
    public static DrawResult Draw(
        List<Awarded> awarded,
        List<Prize> prizes,
        List<Drive> drives,
        List<Car> cars,
        List<PreviousWinner> previousWinners)
    {
        var winners = new List<Drive>();
        var unawarded = new List<Prize>();
        var results = new List<AwardResult>();

        foreach (var prize in prizes)
        {
            var candidates = GetCandidates(drives, prize, awarded, winners, cars, previousWinners);

            if (!candidates.Any())
            {
                // No candidates available, track the prize as unawarded
                unawarded.Add(prize);
                continue;
            }

            var random = new Random();
            var winnerIndex = random.Next(candidates.Count);
            var winner = candidates[winnerIndex];

            winners.Add(winner);
            results.Add(new AwardResult { Prize = prize, Winner = winner });
        }

        return new DrawResult { Awarded = results, Unawarded = unawarded };
    }

    public static List<Drive> GetCandidates(
        List<Drive> drives,
        Prize prize,
        List<Awarded> awarded,
        List<Winner> winners,
        List<Car> cars,
        List<PreviousWinner> previousWinners)
    {
        return drives.Where(drive =>
            (string.IsNullOrEmpty(prize.Race) || prize.Race == drive.Race)
            && HasSticker(cars, drive.Number, prize.Sponsor)
            && !winners.Any(winner => winner.Driver == drive.Driver)
            && !awarded.Any(award => award.Winner.Driver == drive.Driver && award.Prize.Sponsor == prize.Sponsor)
            && !(prize.Sponsor == "Toyo" && HasWonToyo(drive.Driver, previousWinners))
        ).ToList();
    }

    private static bool HasSticker(List<Car> cars, int number, string sponsor)
    {
        // Implement the logic to check if the car has the specified sponsor sticker
        throw new NotImplementedException();
    }

    private static bool HasWonToyo(string driver, List<PreviousWinner> previousWinners)
    {
        // Implement the logic to check if the driver has won a Toyo prize
        throw new NotImplementedException();
    }
}