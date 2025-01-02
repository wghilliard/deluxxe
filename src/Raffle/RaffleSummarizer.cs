namespace Deluxxe.Raffle;

public class RaffleSummarizer
{
    public static Dictionary<string, List<Drive>> GroupResults(List<AwardResult> results)
    {
        return results.Aggregate(new Dictionary<string, List<Drive>>(), (result, entry) =>
        {
            var sponsor = entry.Prize.Sponsor.TrimStart('_');
            var type = entry.Prize.Type;
            var race = entry.Prize.Race;
            var amount = entry.Prize.Amount;
            var frequencyText = !string.IsNullOrEmpty(race) ? $"(Race #{race})" : "(Weekend)";
            var key = $"{sponsor} -- ${amount} {type} {frequencyText}";

            if (!result.ContainsKey(key))
                result[key] = new List<Drive>();

            if (entry.Winner != null)
                result[key].Add(entry.Winner);

            return result;
        });
    }

    public static void PresentResults(List<AwardResult> results)
    {
        var grouped = GroupResults(results);
        var sortedKeys = grouped.Keys.OrderBy(key => key).ToList();

        foreach (var key in sortedKeys)
        {
            var winners = grouped[key];
            if (winners == null || !winners.Any())
            {
                Console.WriteLine($"{key}: Unclaimed");
            }
            else
            {
                var winnersText = string.Join(", ", winners.Select(winner => $"{winner.Driver} #{winner.Number}"));
                Console.WriteLine($"{key}: {winnersText}");
            }
        }
    }

    public static void SummarizeRaffle(List<Prize> prizes, List<Drive> drives, List<AwardResult> awarded)
    {
        var winnerCounts = awarded
            .Select(a => a.Winner.Driver)
            .GroupBy(driver => driver)
            .ToDictionary(group => group.Key, group => group.Count());

        var drivers = drives.Select(drive => drive.Driver).Distinct().ToList();

        Console.WriteLine($"{prizes.Count} prizes");
        Console.WriteLine($"{drives.Count} drives by {drivers.Count} drivers");
        Console.WriteLine($"{winnerCounts.Count} unique winners");

        var realDupes = winnerCounts
            .Where(pair => pair.Value > 1)
            .OrderBy(pair => pair.Value)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        Console.WriteLine($"{realDupes.Count} duplicate winners");

        if (realDupes.Any())
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(realDupes, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }
}