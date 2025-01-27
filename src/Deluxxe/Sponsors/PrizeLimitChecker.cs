using Deluxxe.Extensions;
using Deluxxe.Raffles;

namespace Deluxxe.Sponsors;

public class PrizeLimitChecker
{
    private readonly Dictionary<string, Dictionary<string, int>> _prizeCounts;
    private readonly Dictionary<string, int> _prizeLimits;

    public PrizeLimitChecker(IList<SponsorRecord> sponsorRecords)
    {
        _prizeLimits = sponsorRecords.ToDictionary(GetPrizeKey, record => record.seasonalLimit);
        _prizeCounts = new Dictionary<string, Dictionary<string, int>>();
    }

    public void Update(IList<PrizeWinner> prizeWinners)
    {
        foreach (var prizeWinner in prizeWinners)
        {
            if (!_prizeCounts.TryGetValue(prizeWinner.driver.name, out var value))
            {
                value = new Dictionary<string, int>();
                _prizeCounts.Add(prizeWinner.driver.name, value);
            }

            var prizeKey = GetPrizeKey(prizeWinner.prizeDescription);
            value.TryGetValue(prizeKey, out var prizeCount);

            value[prizeKey] = prizeCount + 1;
        }
    }

    public bool IsBelowLimit(PrizeDescription prize, Driver driver)
    {
        _prizeCounts.TryGetValue(driver.name, out var counts);
        if (counts is null)
        {
            // driver hasn't been awarded any prizes
            return false;
        }

        var prizeKey = GetPrizeKey(prize);
        counts.TryGetValue(prizeKey, out var prizeCount);
        _prizeLimits.TryGetValue(prizeKey, out var seasonLimit);
        if (seasonLimit == 0)
        {
            // no limit set
            return true;
        }

        return prizeCount < seasonLimit;
    }

    private static string GetPrizeKey(SponsorRecord sponsor)
    {
        return GetPrizeKey(sponsor.name, sponsor.sku);
    }

    private static string GetPrizeKey(PrizeDescription prize)
    {
        return GetPrizeKey(prize.sponsorName, prize.sku);
    }

    private static string GetPrizeKey(string sponsorName, string sku)
    {
        return $"{sponsorName.Sanitize()}-{sku}";
    }
}