namespace Deluxxe.Sponsors;

public record SponsorRecord
{
    public required string name { get; init; }
    public required string description { get; init; }
    public required int count { get; init; }
}

public record SponsorRecords
{
    public required List<SponsorRecord> perRacePrizes { get; init; }
    public required List<SponsorRecord> perEventPrizes { get; init; }
}