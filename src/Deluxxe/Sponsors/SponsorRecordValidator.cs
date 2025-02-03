using Deluxxe.Extensions;

namespace Deluxxe.Sponsors;

public static class SponsorRecordValidator
{
    public static IList<Exception> Validate(SponsorRecords records)
    {
        var exceptions = new List<Exception>();
        exceptions.AddRange(Validate(records.perRacePrizes));
        exceptions.AddRange(Validate(records.perEventPrizes));

        return exceptions;
    }

    private static IList<Exception> Validate(IList<SponsorRecord> sponsorRecords)
    {
        var exceptions = new List<Exception>();

        // all records are sane

        foreach (var sponsorRecord in sponsorRecords)
        {
            var aggregateException = sponsorRecord.Validate();
            if (!aggregateException.IsRecordValid)
            {
                exceptions.Add(aggregateException);
            }
        }

        // no skus collide

        var skuSet = new HashSet<string>();

        foreach (var sponsorRecord in sponsorRecords)
        {
            var uniqueSku = $"{sponsorRecord.name.Sanitize()}-{sponsorRecord.sku}";

            if (!skuSet.Add(uniqueSku))
            {
                exceptions.Add(new SponsorRecordException(sponsorRecord, $"duplicate sku found sku={uniqueSku}"));
            }
        }

        return exceptions;
    }
}