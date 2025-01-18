using Bogus;
using Deluxxe.ModelsV3;
using Deluxxe.Raffles;
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

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winner);
        _testOutputHelper.WriteLine(winner.ToString());

        Assert.True(given.CarToStickerMap[winner.Driver.CarNumber][winner.PrizeDescription.SponsorName]);
    }

    [Fact]
    public void DrawPrize_DriverDoesNotHaveSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithNoStickers()
            .Build();

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
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

        var firstWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(firstWinner);
        _testOutputHelper.WriteLine(firstWinner.ToString());

        var previousWinners = new List<PrizeWinner<PrizeDescription>>();
        previousWinners.AddRange(given.PreviousWinners);
        previousWinners.Add(firstWinner);

        var secondWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, previousWinners);
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

        var (winners, notAwarded) = GetPrizeRaffle(given).DrawPrizes(given.PrizeDescriptions, given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winners);
        Assert.Equal(2, winners.Count);

        Assert.NotEqual(winners[0].Driver.Name, winners[1].Driver.Name);

        Assert.NotNull(notAwarded);
        Assert.Empty(notAwarded);
    }

    [Fact]
    public void DrawPrizes_MorePrizesThanDrivers()
    {
        var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var (winners, notAwarded) = GetPrizeRaffle(given).DrawPrizes(given.PrizeDescriptions, given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winners);
        Assert.Single(winners);

        Assert.NotNull(notAwarded);
        Assert.Single(notAwarded);
    }

    private static TestHarnessBuilder Given()
    {
        return new TestHarnessBuilder();
    }

    public PrizeRaffle<PrizeDescription> GetPrizeRaffle(TestHarness testHarness)
    {
        return new PrizeRaffle<PrizeDescription>(loggerFactory.CreateLogger<PrizeRaffle<PrizeDescription>>(), activitySource, testHarness.GetStickerManager());
    }

    public class TestHarnessBuilder
    {
        private readonly List<PrizeDescription> _prizeDescriptions = [];
        private List<Driver> _drivers = [];
        private IDictionary<string, string> _driverToCarMap = new Dictionary<string, string>();
        private readonly Dictionary<string, IDictionary<string, bool>> _carToStickerMap = new();

        private readonly IList<RaceResult> _raceResults = new List<RaceResult>();
        private readonly IList<PrizeWinner<PrizeDescription>> _previousWinners = new List<PrizeWinner<PrizeDescription>>();

        private readonly Random _random = new();

        private static readonly string[] MostSponsorNames = ["_425", "AAF", "Alpinestars", "Bimmerworld", "Griots", "Redline", "RoR"];

        private static readonly PrizeDescription ToyoPrize = new()
        {
            SponsorName = Constants.ToyoTiresSponsorName,
            Description = "4 toyo tires"
        };

        private readonly Faker<Driver> _driverFaker = new Faker<Driver>()
            .RuleFor(a => a.Name, f => f.Name.FullName())
            .RuleFor(a => a.CarNumber, f => f.Random.Number(1, 100).ToString())
            .RuleFor(a => a.Email, f => f.Lorem.Word() + "@nyan.cat");

        private readonly Faker<PrizeDescription> _prizeFaker = new Faker<PrizeDescription>()
            .RuleFor(a => a.SponsorName, f => f.PickRandom(MostSponsorNames))
            .RuleFor(a => a.Description, f => f.Lorem.Sentence());

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
            _driverToCarMap = _drivers.ToDictionary(driver => driver.Name, driver => driver.CarNumber);

            if (!allCarsMapped)
            {
                _driverToCarMap.Remove(_drivers[_random.Next(_drivers.Count)].Name);
            }

            foreach (var driver in _drivers)
            {
                _raceResults.Add(new RaceResult()
                {
                    Driver = driver,
                    CarClass = "PRO3",
                    Position = _random.Next(1, 5),
                    RaceId = 1,
                    Gap = allDriversStarted ? "00:00:00" : _random.Next(1) == 1 ? "00:00:00" : "DNS",
                });
            }

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
                _previousWinners.Add(new PrizeWinner<PrizeDescription>
                {
                    Driver = _drivers.First(),
                    PrizeDescription = ToyoPrize
                });
                count--;
            }

            if (count == 0)
            {
                return this;
            }

            for (var i = 0; i < count; i++)
            {
                _previousWinners.Add(new PrizeWinner<PrizeDescription>
                {
                    Driver = _drivers[_random.Next(_drivers.Count)],
                    PrizeDescription = _prizeDescriptions[_random.Next(_prizeDescriptions.Count)]
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

                _carToStickerMap[car][ToyoPrize.SponsorName] = allStickersMapped || _random.Next(1) != 1;
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

                _carToStickerMap[car][ToyoPrize.SponsorName] = false;
            }

            return this;
        }

        public TestHarness Build()
        {
            return new TestHarness
            {
                PrizeDescriptions = _prizeDescriptions,
                Drivers = _drivers,
                RaceResults = _raceResults,
                PreviousWinners = _previousWinners,
                CarToStickerMap = _carToStickerMap
            };
        }
    }

    public record TestHarness
    {
        public required IList<PrizeDescription> PrizeDescriptions;
        public required IList<Driver> Drivers;
        public required IList<RaceResult> RaceResults;
        public required IList<PrizeWinner<PrizeDescription>> PreviousWinners;
        public required IDictionary<string, IDictionary<string, bool>> CarToStickerMap;

        public IStickerManager GetStickerManager()
        {
            return new InMemoryStickerManager(CarToStickerMap);
        }
    }
}