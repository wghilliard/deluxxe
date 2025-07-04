namespace Deluxxe.Raffles;

public record PrizeDescription
{
    public required string sponsorName { get; init; }
    public required string description { get; init; }
    public required string sku { get; init; }

    public required string serial { get; init; }
}