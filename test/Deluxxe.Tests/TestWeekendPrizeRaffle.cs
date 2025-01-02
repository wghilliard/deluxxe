using Bogus;
using Deluxxe.ModelsV3;
using Deluxxe.Raffles;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Deluxxe.Tests;

public class TestWeekendPrizeRaffle(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<WeekendPrizeRaffle> _logger = XUnitLogger.CreateLogger<WeekendPrizeRaffle>(testOutputHelper);

    [Fact]
    public void DrawPrize_DriverHasSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var winner = new WeekendPrizeRaffle(_logger, given.GetStickerManager()).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winner);
        testOutputHelper.WriteLine(winner.ToString());

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

        var winner = new WeekendPrizeRaffle(_logger, given.GetStickerManager()).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
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

        var firstWinner = new WeekendPrizeRaffle(_logger, given.GetStickerManager()).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(firstWinner);
        testOutputHelper.WriteLine(firstWinner.ToString());

        var previousWinners = new List<WeekendPrizeWinner>();
        previousWinners.AddRange(given.PreviousWinners);
        previousWinners.Add(firstWinner);

        var secondWinner = new WeekendPrizeRaffle(_logger, given.GetStickerManager()).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, previousWinners);
        Assert.Null(secondWinner);
    }

    TestHarnessBuilder Given()
    {
        return new TestHarnessBuilder();
    }

    public class TestHarnessBuilder
    {
        private readonly IList<WeekendPrizeDescription> _prizeDescriptions = new List<WeekendPrizeDescription>();
        private IList<Driver> _drivers = new List<Driver>();
        private IDictionary<string, string> _driverToCarMap = new Dictionary<string, string>();
        private IDictionary<string, IDictionary<string, bool>> _carToStickerMap = new Dictionary<string, IDictionary<string, bool>>();

        private readonly IList<RaceResult> _raceResults = new List<RaceResult>();
        private readonly IList<WeekendPrizeWinner> _previousWinners = new List<WeekendPrizeWinner>();

        private readonly Random _random = new();

        private static readonly string[] MostSponsorNames = ["_425", "AAF", "Alpinestars", "Bimmerworld", "Griots", "Redline", "RoR"];

        private static readonly WeekendPrizeDescription ToyoPrize = new()
        {
            SponsorName = "Toyo",
            Description = "4 toyo tires"
        };

        private readonly Faker<Driver> _driverFaker = new Faker<Driver>()
            .RuleFor(a => a.Name, f => f.Name.FullName())
            .RuleFor(a => a.CarNumber, f => f.Random.Number(1, 100).ToString())
            .RuleFor(a => a.Email, f => f.Lorem.Word() + "@nyan.cat");

        private readonly Faker<WeekendPrizeDescription> _prizeFaker = new Faker<WeekendPrizeDescription>()
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
                _previousWinners.Add(new WeekendPrizeWinner
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
                _previousWinners.Add(new WeekendPrizeWinner
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
            return new TestHarness()
            {
                Drivers = _drivers as IReadOnlyList<Driver>,
                PrizeDescriptions = _prizeDescriptions as IReadOnlyList<WeekendPrizeDescription>,
                RaceResults = _raceResults as IReadOnlyList<RaceResult>,
                PreviousWinners = _previousWinners as IReadOnlyList<WeekendPrizeWinner>,
                DriverToCarMap = _driverToCarMap as IReadOnlyDictionary<string, string>,
                CarToStickerMap = _carToStickerMap
            };
        }
    }

    public record TestHarness
    {
        public IReadOnlyList<WeekendPrizeDescription> PrizeDescriptions;
        public IReadOnlyList<Driver> Drivers;
        public IReadOnlyList<RaceResult> RaceResults;
        public IReadOnlyList<WeekendPrizeWinner> PreviousWinners;
        public IReadOnlyDictionary<string, string> DriverToCarMap;
        public IDictionary<string, IDictionary<string, bool>> CarToStickerMap;

        public StickerManager GetStickerManager()
        {
            return new StickerManager(DriverToCarMap, CarToStickerMap);
        }
    }
}