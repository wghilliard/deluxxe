// using Deluxxe.Models;
//
// namespace Deluxxe.Raffles;
//
// public class Raffle
// {
//     public static RoundResults Draw(
//         List<PrizeAward> awarded,
//         List<Prize> prizes,
//         List<RaceResult> drives,
//         Dictionary<int, Car> cars,
//         Dictionary<int, List<PrizeWinner>> previousWinners)
//     {
//         var winners = new List<RaceResult>();
//         var unawarded = new List<Prize>();
//         var results = prizes.Aggregate(new List<PrizeAward>(), (result, prize) =>
//         {
//             var candidates = GetCandidates(drives, prize, awarded, winners, cars, previousWinners);
//             if (!candidates.Any())
//             {
//                 // No candidates available, track the prize as unawarded
//                 unawarded.Add(prize);
//                 return result;
//             }
//
//             var random = new Random();
//             var winnerIndex = random.Next(candidates.Count);
//             var winner = candidates[winnerIndex];
//
//             winners.Add(winner);
//             result.Add(new PrizeAward { Prize = prize, Winner = winner });
//             return result;
//         });
//
//         return new RoundResults(awarded: results, unawarded: unawarded);
//     }
//
//     public static List<RaceResult> GetCandidates(
//         List<RaceResult> drives,
//         Prize prize,
//         List<PrizeAward> awarded,
//         List<RaceResult> winners,
//         Dictionary<int, Car> cars,
//         Dictionary<int, List<PrizeWinner>> previousWinners)
//     {
//         return drives.Where(drive =>
//             (!prize.RaceId.HasValue || prize.RaceId == drive.RaceId)
//             && HasSticker(cars, drive.Number, prize.SponsorName)
//             && !winners.Any(winner => winner.DriverName == drive.DriverName)
//             && !awarded.Any(award =>
//                 award.Winner.DriverName == drive.DriverName && award.Prize.SponsorName == prize.SponsorName)
//             && !(prize.SponsorName == "Toyo" && HasWonToyo(drive.DriverName, previousWinners))
//         ).ToList();
//     }
//
//     private static bool HasSticker(Dictionary<int, Car> cars, int number, string sponsorName)
//     {
//         // Implement logic to check if the car has the specified sponsor's sticker
//         throw new NotImplementedException();
//     }
//
//     private static bool HasWonToyo(string driverName, Dictionary<int, List<PrizeWinner>> previousWinners)
//     {
//         // Implement logic to check if the driver has won a Toyo prize
//         throw new NotImplementedException();
//     }
// }