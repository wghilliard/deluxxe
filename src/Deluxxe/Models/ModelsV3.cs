namespace Deluxxe.ModelsV3;

public record RaceResult
{
    public required Driver Driver;
    public int Position;
    // public required string Number;
    public int RaceId;
    public required string CarClass;
    // gap from leader? it might be DNS
    public required string Gap;
}

public record Driver
{
    public required string Name;
    public required string Email;
    public required string CarNumber;
}

public record PrizeDescription
{
    public required string SponsorName;
    public required string Description;
}

public record PrizeWinner<T>
{
    public required T PrizeDescription;
    public required Driver Driver;
}