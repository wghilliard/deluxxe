using Deluxxe.ModelsV3;
using Deluxxe.Raffles;

namespace Deluxxe.Tests;

public class TestWeekendPrizeRaffle
{
    private readonly IList<WeekendPrizeDescription> _mockPrizeDescriptions = new List<WeekendPrizeDescription>
    {
        new()
        {
            SponsorName = "Toyo",
            Description = "4 toyo tires"
        },
        new()
        {
            SponsorName = "Bimmerworld",
            Description = "$250"
        }
    };

    private readonly IList<Driver> _mockDrivers = new List<Driver>
    {
        new()
        {
            FirstName = "Caleb",
            LastName = "Trask",
            CarNumber = "168",
            Email = "caleb@nyan.cat"
        },
        new()
        {
            FirstName = "Aron",
            LastName = "Trask",
            CarNumber = "101",
            Email = "aron@nyan.cat"
        }
    };


    [Fact]
    public void DriverGetsPrize()
    {
        var winners = WeekendPrizeRaffle.DrawPrizes(_mockPrizeDescriptions, _mockDrivers);
    }
}