using Bogus;
using Deluxxe.Raffles;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Deluxxe.Tests.Raffles;

public class TestPrizeRaffle(ITestOutputHelper testOutputHelper) : BaseTest(testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void DrawPrize_DriverHasSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.Drivers, given.PreviousWinners, given.raceConfig);
        Assert.NotNull(winner);
        _testOutputHelper.WriteLine(winner.ToString());

        Assert.True(given.CarToStickerMap[winner.driver.carNumber][winner.prizeDescription.sponsorName]);
    }

    [Fact]
    public void DrawPrize_DriverDoesNotHaveSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithNoStickers()
            .Build();

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.Drivers, given.PreviousWinners, given.raceConfig);
        Assert.Null(winner);
    }

    [Fact]
    public void DrawPrize_DriverPreviousWon()
    {
        var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(1, true)
            .WithStickers()
            .Build();

        var firstWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.Drivers, given.PreviousWinners, given.raceConfig);
        Assert.NotNull(firstWinner);
        _testOutputHelper.WriteLine(firstWinner.ToString());

        var previousWinners = new List<PrizeWinner>();
        previousWinners.AddRange(given.PreviousWinners);
        previousWinners.Add(firstWinner);

        var secondWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.Drivers, previousWinners, given.raceConfig);
        Assert.Null(secondWinner);
    }

    [Fact]
    public void DrawPrizes_AllPrizesAwarded()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var result = GetPrizeRaffle(given).DrawPrizes(given.PrizeDescriptions, given.Drivers, given.PreviousWinners, given.raceConfig, 1);
        Assert.NotNull(result.winners);
        Assert.Equal(2, result.winners.Count);

        Assert.NotEqual(result.winners[0].driver.name, result.winners[1].driver.name);

        Assert.NotNull(result.notAwarded);
        Assert.Empty(result.notAwarded);
    }

    [Fact]
    public void DrawPrizes_MorePrizesThanDrivers()
    {
        var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var result = GetPrizeRaffle(given).DrawPrizes(given.PrizeDescriptions, given.Drivers, given.PreviousWinners, given.raceConfig, 1);
        Assert.NotNull(result.winners);
        Assert.Single(result.winners);

        Assert.NotNull(result.notAwarded);
        Assert.Single(result.notAwarded);
    }

    private static TestHarnessBuilder Given()
    {
        return new TestHarnessBuilder();
    }

    private PrizeRaffle GetPrizeRaffle(TestHarness testHarness)
    {
        return new PrizeRaffle(loggerFactory.CreateLogger<PrizeRaffle>(), activitySource, testHarness.GetStickerManager(), new Random(1337));
    }

    public class TestHarnessBuilder
    {
        private readonly List<PrizeDescription> _prizeDescriptions = [];
        private List<Driver> _drivers = [];
        private IDictionary<string, string> _driverToCarMap = new Dictionary<string, string>();
        private readonly Dictionary<string, IDictionary<string, bool>> _carToStickerMap = new();

        private readonly IList<PrizeWinner> _previousWinners = new List<PrizeWinner>();

        private readonly Random _random = new(1337);

        private static readonly string[] MostSponsorNames = ["_425", "AAF", "Alpinestars", "Bimmerworld", "Griots", "Redline", "RoR"];

        private static readonly PrizeDescription ToyoPrize = new()
        {
            sponsorName = SponsorConstants.ToyoTires,
            description = "4 toyo tires",
            sku = "1"
        };

        private readonly Faker<Driver> _driverFaker = new Faker<Driver>()
            .RuleFor(a => a.name, f => f.Name.FullName())
            .RuleFor(a => a.carNumber, f => f.Random.Number(1, 100).ToString());

        private readonly Faker<PrizeDescription> _prizeFaker = new Faker<PrizeDescription>()
            .RuleFor(a => a.sponsorName, f => f.PickRandom(MostSponsorNames))
            .RuleFor(a => a.description, f => f.Lorem.Sentence())
            .RuleFor(a => a.sku, f => f.Lorem.Word());

        public TestHarnessBuilder WithDrivers(int count, bool allDriversStarted = true, bool allCarsMapped = true)
        {
            if (count == 0)
            {
                return this;
            }

            for (var i = 0; i < count; i++)
            {
                _drivers.Add(_driverFaker.Generate());
            }

            _drivers = _drivers.OrderBy(_ => _random.Next()).ToList();
            _driverToCarMap = _drivers.ToDictionary(driver => driver.name, driver => driver.carNumber);

            if (!allCarsMapped)
            {
                _driverToCarMap.Remove(_drivers[_random.Next(_drivers.Count)].name);
            }

            // foreach (var driver in _drivers)
            // {
            //     _raceResults.Add(new RaceResult()
            //     {
            //         Driver = driver,
            //         CarClass = "PRO3",
            //         Position = _random.Next(1, 5),
            //         RaceId = 1,
            //         Gap = allDriversStarted ? "00:00:00" : _random.Next(1) == 1 ? "00:00:00" : "DNS",
            //     });
            // }

            return this;
        }

        public TestHarnessBuilder WithPrizeDescriptions(int count, bool withToyo)
        {
            if (count == 0)
            {
                return this;
            }

            if (withToyo)
            {
                _prizeDescriptions.Add(ToyoPrize);
                count--;
            }

            if (count == 0)
            {
                return this;
            }

            for (var i = 0; i < count; i++)
            {
                _prizeDescriptions.Add(_prizeFaker.Generate());
            }

            return this;
        }

        public TestHarnessBuilder WithPreviousWinners(int count, bool withToyo)
        {
            if (count == 0)
            {
                return this;
            }

            if (withToyo)
            {
                _previousWinners.Add(new PrizeWinner
                {
                    driver = _drivers.First(),
                    prizeDescription = ToyoPrize,
                    resourceId = "nyan-counting"
                });
                count--;
            }

            if (count == 0)
            {
                return this;
            }

            for (var i = 0; i < count; i++)
            {
                _previousWinners.Add(new PrizeWinner
                {
                    driver = _drivers[_random.Next(_drivers.Count)],
                    prizeDescription = _prizeDescriptions[_random.Next(_prizeDescriptions.Count)],
                    resourceId = "nyan-counting"
                });
            }

            return this;
        }

        public TestHarnessBuilder WithStickers(bool allCarsMapped = true, bool allStickersMapped = true)
        {
            foreach (var car in _driverToCarMap.Values)
            {
                _carToStickerMap[car] = new Dictionary<string, bool>();
                foreach (var sponsor in MostSponsorNames)
                {
                    _carToStickerMap[car][sponsor] = allStickersMapped || _random.Next(1) != 1;
                }

                _carToStickerMap[car][ToyoPrize.sponsorName] = allStickersMapped || _random.Next(1) != 1;
            }

            return this;
        }

        public TestHarnessBuilder WithNoStickers()
        {
            foreach (var car in _driverToCarMap.Values)
            {
                _carToStickerMap[car] = new Dictionary<string, bool>();
                foreach (var sponsor in MostSponsorNames)
                {
                    _carToStickerMap[car][sponsor] = false;
                }

                _carToStickerMap[car][ToyoPrize.sponsorName] = false;
            }

            return this;
        }

        public TestHarness Build()
        {
            return new TestHarness
            {
                PrizeDescriptions = _prizeDescriptions,
                Drivers = _drivers,
                PreviousWinners = _previousWinners,
                CarToStickerMap = _carToStickerMap
            };
        }
    }

    public record TestHarness
    {
        public required IList<PrizeDescription> PrizeDescriptions;
        public required IList<Driver> Drivers;
        public required IList<PrizeWinner> PreviousWinners;
        public required IDictionary<string, IDictionary<string, bool>> CarToStickerMap;

        public DrawingConfiguration eventConfig = new()
        {
            DrawingType = DrawingType.Event,
            Season = "2025",
            ResourceIdBuilder = new ResourceIdBuilder()
                .WithSeason("2025")
                .WithEvent("spring-into-summer", "123")
                .WithEventDrawingRound("1")
        };

        public DrawingConfiguration raceConfig = new()
        {
            DrawingType = DrawingType.Event,
            Season = "2025",
            ResourceIdBuilder = new ResourceIdBuilder()
                .WithSeason("2025")
                .WithEvent("spring-into-summer", "123")
                .WithRaceDrawingRound("saturday-race", "1234", "1")
        };

        public IStickerManager GetStickerManager()
        {
            return new InMemoryStickerManager(CarToStickerMap);
        }
    }
}