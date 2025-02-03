namespace Deluxxe.Sponsors;

public record SponsorRecord
{
    public required string name { get; init; }
    public required string description { get; init; }
    public required int count { get; init; }
    public required string sku { get; init; }
    public int seasonalLimit { get; init; }

    public SponsorRecordAggregateException Validate()
    {
        var exceptions = new List<Exception>();

        if (string.IsNullOrWhiteSpace(name))
        {
            exceptions.Add(new ArgumentException("name is required"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            exceptions.Add(new ArgumentException("description is required"));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            exceptions.Add(new ArgumentException("sku is required"));
        }

        if (seasonalLimit < 0)
        {
            exceptions.Add(new ArgumentException("seasonal limit must be equal to or greater than zero"));
        }

        return new SponsorRecordAggregateException(this, exceptions);
    }
}

public class SponsorRecordAggregateException(SponsorRecord sponsorRecord, IList<Exception> exceptions) : AggregateException(exceptions)
{
    public bool IsRecordValid => InnerExceptions.Count == 0;

    public override string ToString()
    {
        return $"[sponsorRecord={sponsorRecord}][isRecordValid={IsRecordValid}][exceptions={base.ToString()}]";
    }
}

public class SponsorRecordException(SponsorRecord SponsorRecord, string message) : Exception(message);

public record SponsorRecords
{
    public required List<SponsorRecord> perRacePrizes { get; init; }
    public required List<SponsorRecord> perEventPrizes { get; init; }
}