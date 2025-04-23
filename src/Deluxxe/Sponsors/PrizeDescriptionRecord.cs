namespace Deluxxe.Sponsors;

public readonly struct PrizeDescriptionRecord
{
    public required string name { get; init; }
    public required string description { get; init; }
    public required int count { get; init; }
    public required string sku { get; init; }
    public int seasonalLimit { get; init; }

    public PrizeDescriptionRecordAggregateException Validate()
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

        return new PrizeDescriptionRecordAggregateException(this, exceptions);
    }
}

public class PrizeDescriptionRecordAggregateException(PrizeDescriptionRecord prizeDescriptionRecord, IList<Exception> exceptions) : AggregateException(exceptions)
{
    public bool isRecordValid => InnerExceptions.Count == 0;

    public override string ToString()
    {
        return $"[sponsorRecord={prizeDescriptionRecord}][isRecordValid={isRecordValid}][exceptions={base.ToString()}]";
    }
}

public class PrizeDescriptionRecordException(PrizeDescriptionRecord prizeDescriptionRecord, string message) : Exception(message);

public struct PrizeDescriptionRecords
{
    public required List<PrizeDescriptionRecord> perRacePrizes { get; init; }
    public required List<PrizeDescriptionRecord> perEventPrizes { get; init; }
}