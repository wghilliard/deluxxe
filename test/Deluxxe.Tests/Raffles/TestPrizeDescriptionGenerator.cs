using Bogus;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;

namespace Deluxxe.Tests.Raffles;

public class TestPrizeDescriptionGenerator
{
    private readonly Faker<Driver> _driverFaker = new Faker<Driver>()
        .RuleFor(a => a.name, f => f.Name.FullName())
        .RuleFor(a => a.carNumber, f => f.Random.Number(1, 100).ToString());

    [Fact]
    public void TestCalculatePrizeValue_ValidMap_Success()
    {
        var value = PrizeDescriptionGenerator.CalculatePrizeValue(ValueFunc.CountAtOrBelow,
            new Dictionary<string, string>
            {
                { "1", "1value" },
                { "2", "2value" },
                { "3", "3value" }
            },
            new List<Driver>
            {
                new()
                {
                    name = "Driver1",
                    carNumber = "01"
                },
                new()
                {
                    name = "Driver2",
                    carNumber = "02"
                }
            });

        Assert.Equal("2value", value);
    }

    [Fact]
    public void TestCalculatePrizeValue_ValidMapWithRange_Success()
    {
        var drivers = new List<Driver>();
        for (var i = 0; i < 10; i++)
        {
            drivers.Add(_driverFaker.Generate());
        }

        var value = PrizeDescriptionGenerator.CalculatePrizeValue(ValueFunc.CountAtOrBelow,
            new Dictionary<string, string>
            {
                { "1", "1value" },
                { "2", "2value" },
                { "3", "3value" }
            },
            drivers);

        Assert.Equal("3value", value);
    }

    [Theory]
    [InlineData(14, "265")]
    [InlineData(30, "550")]
    [InlineData(31, "600")]
    public void TestCalculatePrizeValue_ValidMapWithGaps_Success(int driverCount, string expectedValue)
    {
        var drivers = new List<Driver>();
        for (var i = 0; i < driverCount; i++)
        {
            drivers.Add(_driverFaker.Generate());
        }

        var value = PrizeDescriptionGenerator.CalculatePrizeValue(ValueFunc.CountAtOrBelow,
            new Dictionary<string, string>
            {
                { "100", "600" },
                { "30", "550" },
                { "24", "440" },
                { "20", "355" },
                { "15", "265" },
                { "10", "175" },
                { "5", "85" },
                { "2", "0" }
            },
            drivers);

        Assert.Equal(expectedValue, value);
    }
}