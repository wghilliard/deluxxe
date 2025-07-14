namespace Deluxxe.Raffles;

public struct RaffleResult
{
    public required IList<DrawingResult> drawings { get; init; }
    public required string resourceId { get; init; }
    public required string name { get; init; }
    public required string season { get; init; }
    public required string configurationName { get; init; }
    public string? trackName { get; init; }
}

public struct DrawingResult
{
    public required IList<PrizeWinner> winners { get; init; }
    public required IList<PrizeDescription> notAwarded { get; init; }
    public required DrawingType drawingType { get; init; }

    public string? startTime { get; init; }
    public int? eligibleCandidatesCount { get; init; }
}

public struct DrawingRoundResult
{
    public required IList<PrizeWinner> winners { get; init; }
    public required IList<PrizeDescription> notAwarded { get; init; }
}

public record Driver : IComparable<Driver>
{
    public required string name { get; init; }
    public required string carNumber { get; init; }

    public int CompareTo(Driver? other)
    {
        return StringComparer.OrdinalIgnoreCase.Compare(name, other!.name);
    }
}

public record PrizeWinner
{
    public required PrizeDescription prizeDescription { get; init; }
    public required DrawingCandidate candidate { get; init; }
    public required string resourceId { get; init; }
}