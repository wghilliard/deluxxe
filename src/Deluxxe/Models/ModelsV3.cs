namespace Deluxxe.ModelsV3;

public class Driver
{
    public required string FirstName;
    public required string LastName;
    public required string Email;
    public required string CarNumber;
}

public class WeekendPrizeDescription
{
    public string SponsorName { get; set; }
    public string Description { get; set; }
}

public class WeekendPrizeWinner
{
    public required WeekendPrizeDescription prizeDescription;
    public required Driver driver;
}