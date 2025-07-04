namespace Deluxxe.Raffles;

using Deluxxe.Sponsors;

public static class PrizeDescriptionGenerator
{
    public static List<PrizeDescription> GeneratePrizeDescriptions(IList<PrizeDescriptionRecord> prizeDescriptionRecords, IList<Driver> eligibleDrivers)
    {
        var prizeDescriptions = new List<PrizeDescription>();

        foreach (var record in prizeDescriptionRecords)
        {
            for (var count = 0; count < record.count; count++)
            {
                string description;
                if (record.valueFunc is not ValueFunc.Empty)
                {
                    var value = CalculatePrizeValue(record.valueFunc, record.valueMap, eligibleDrivers);
                    description = string.Format(record.description, value);
                }
                else
                {
                    description = record.description;
                }

                prizeDescriptions.Add(new PrizeDescription
                {
                    description = description,
                    sponsorName = record.name,
                    sku = record.sku,
                    serial = (count + 1).ToString()
                });
            }
        }

        return prizeDescriptions;
    }

    public static string CalculatePrizeValue(ValueFunc prizeValueFunc, IDictionary<string, string> valueMap, IList<Driver> eligibleDrivers)
    {
        if (prizeValueFunc is ValueFunc.CountAtOrBelow)
        {
            var count = eligibleDrivers.Count;
            var keys = valueMap.Keys.Select(int.Parse).ToList();
            keys.Sort();

            var values = keys.Select(key => valueMap[key.ToString()]).ToList();

            var lowerBound = 0;
            var upperBound = keys[0];
            for (var keyIndex = 0; keyIndex < keys.Count - 1; keyIndex++)
            {
                if (lowerBound < count && count <= upperBound)
                {
                    return values[keyIndex];
                }

                lowerBound = keys[keyIndex];
                upperBound = keys[keyIndex + 1];
            }

            return values[^1];
        }

        throw new ArgumentException("unknown value function or value map");
    }
}