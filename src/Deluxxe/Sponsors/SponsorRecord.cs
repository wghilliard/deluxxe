namespace Deluxxe.Sponsors;

public record SponsorRecord
{
    public required string name { get; set; }
    public required string description { get; set; }
    public required int count { get; set; }
}

public record SponsorRecords
{
    public required List<SponsorRecord> perRacePrizes { get; set; }
    public required List<SponsorRecord> perEventPrizes { get; set; }
}