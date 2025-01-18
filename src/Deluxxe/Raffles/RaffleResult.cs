namespace Deluxxe.Raffles;

public record RaffleResult
{
    public required IList<DrawingResult> drawings { get; init; }
    public required string eventId { get; init; }
}

public record DrawingResult
{
    public required IList<PrizeWinner> winners { get; init; }
    public required IList<PrizeDescription> notAwarded { get; init; }
    public required DrawingType drawingType { get; init; }
    public required int randomSeed { get; init; }
}

public record Driver
{
    public required string name { get; init; }
    public required string carNumber { get; init; }
}

public record PrizeDescription
{
    public required string sponsorName { get; init; }
    public required string description { get; init; }
}

public record PrizeWinner
{
    public required PrizeDescription prizeDescription { get; init; }
    public required Driver driver { get; init; }
    public required int seasonAwarded { get; init; }
    public required string eventId { get; init; }
}