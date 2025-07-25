﻿using System.Security.Cryptography;
using Bogus;
using Deluxxe.Raffles;
using Deluxxe.Resources;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Deluxxe.Tests.Raffles;

public class TestPrizeRaffle(ITestOutputHelper testOutputHelper) : BaseTest(testOutputHelper)
{
    private const string CarOwner = "car-owner";
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void DrawPrize_DriverHasSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.DriversAsCandidates(false), given.PreviousWinners, given.raceConfig);
        Assert.NotNull(winner);
        _testOutputHelper.WriteLine(winner.ToString());

        Assert.True(given.CarToStickerMap[winner.candidate.carNumber][winner.prizeDescription.sponsorName.ToLower()]);
    }

    [Fact]
    public void DrawPrize_DriverDoesNotHaveSticker()
    {
        var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithNoStickers()
            .Build();

        var winner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.DriversAsCandidates(false), given.PreviousWinners, given.raceConfig);
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

        var firstWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.DriversAsCandidates(false), given.PreviousWinners, given.raceConfig);
        Assert.NotNull(firstWinner);
        _testOutputHelper.WriteLine(firstWinner.ToString());

        var previousWinners = new List<PrizeWinner>();
        previousWinners.AddRange(given.PreviousWinners);
        previousWinners.Add(firstWinner);

        var secondWinner = GetPrizeRaffle(given).DrawPrize(given.PrizeDescriptions[0], given.DriversAsCandidates(false), previousWinners, given.raceConfig);
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

        Assert.NotEqual(result.winners[0].candidate.name, result.winners[1].candidate.name);

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

    [Fact]
    public void DrawPrizes_RentalsAllowedToReceivePrizes()
    {
        var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .WithRentals()
            .Build();

        var result = GetPrizeRaffle(given, allowRentals: true).DrawPrizes(given.PrizeDescriptions, given.Drivers, given.PreviousWinners, given.raceConfig, 1);
        Assert.NotNull(result.winners);
        Assert.Single(result.winners);
        Assert.False(result.winners[0].candidate.name == CarOwner);

        Assert.NotNull(result.notAwarded);
        Assert.Single(result.notAwarded);
    }

    [Fact]
    public void DrawPrizes_RentalsNotAllowedToReceivePrizes()
    {
        var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .WithRentals()
            .Build();

        var result = GetPrizeRaffle(given, allowRentals: false).DrawPrizes(given.PrizeDescriptions, given.Drivers, given.PreviousWinners, given.raceConfig, 1);
        Assert.NotNull(result.winners);
        Assert.Single(result.winners);
        Assert.True(result.winners[0].candidate.name == CarOwner);

        Assert.NotNull(result.notAwarded);
        Assert.Single(result.notAwarded);
    }

    [Theory]
    [InlineData(24, 14, 8, 36.415)]
    [InlineData(30, 14, 8, 42.56)]
    [InlineData(30, 10000, 8, 42.56)]
    // https://math.arizona.edu/~jwatkins/chi-square-table.pdf
    public void DrawPrizes_ResultsAreRandom(int nDrivers, int nDrawings, int nPrizesPerDrawing, double criticalValue)
    {
        var given = Given()
            .WithDrivers(nDrivers, allCarsMapped: true)
            .WithPrizeDescriptions(nPrizesPerDrawing, withToyo: false)
            .WithStickers(allCarsMapped: true, allStickersMapped: true)
            .Build();

        var aggregatedResults = new Dictionary<string, int>();
        var candidateMapping = new Dictionary<string, int>();
        var driverPerSponsorWinCount = new Dictionary<string, Dictionary<string, int>>();

        var samples = new List<int>();

        int candidateCounter = 0;

        foreach (var driver in given.Drivers)
        {
            candidateMapping[driver.name] = candidateCounter;
            candidateCounter++;
        }

        for (var round = 0; round < nDrawings; round++)
        {
            var result = GetPrizeRaffle(given).DrawPrizes(given.PrizeDescriptions, given.Drivers, given.PreviousWinners, given.raceConfig, round);
            foreach (var winner in result.winners)
            {
                if (!aggregatedResults.TryAdd(winner.candidate.name, 1))
                {
                    aggregatedResults[winner.candidate.name] += 1;
                }

                if (!driverPerSponsorWinCount.TryGetValue(winner.candidate.name, out var sponsorCount))
                {
                    driverPerSponsorWinCount[winner.candidate.name] = sponsorCount = new Dictionary<string, int>();
                }

                if (!sponsorCount.TryAdd(winner.prizeDescription.sponsorName, 1))
                {
                    sponsorCount[winner.prizeDescription.sponsorName] += 1;
                }

                samples.Add(candidateMapping[winner.candidate.name]);
            }
        }

        Assert.Equal(nDrawings * nPrizesPerDrawing, aggregatedResults.Select(x => x.Value).Sum());

        const int min = 0;
        int[] observed = new int[nDrivers];

        foreach (var driverIndex in samples)
        {
            observed[driverIndex - min]++;
        }

        double expected = samples.Count / (double)nDrivers;
        double chiSquare = 0.0;

        // Calculate Chi-Square statistic
        for (int i = 0; i < nDrivers; i++)
        {
            double diff = observed[i] - expected;
            chiSquare += diff * diff / expected;
        }

        foreach (var (driver, wins) in driverPerSponsorWinCount)
        {
            var minWin = wins.MinBy(x => x.Value);
            var maxWin = wins.MaxBy(x => x.Value);
            var avgWin = wins.Average(x => x.Value);
            _testOutputHelper.WriteLine($"Driver {driver} min wins: {minWin.Value}/{maxWin.Value}/{avgWin} - {minWin.Key}/{maxWin.Key}");
        }

        _testOutputHelper.WriteLine($"degrees of freedom: {nDrivers - 1}, chi square: {chiSquare}, criticalValue: {criticalValue}, expectedValue:{expected}");
        Assert.True(chiSquare < criticalValue);
    }

    private static TestHarnessBuilder Given()
    {
        return new TestHarnessBuilder();
    }

    private PrizeRaffle GetPrizeRaffle(TestHarness testHarness, bool allowRentals = false)
    {
        return new PrizeRaffle(loggerFactory.CreateLogger<PrizeRaffle>(),
            activitySource,
            testHarness.GetStickerManager(allowRentals),
            testHarness.GetPrizeLimitChecker(),
            RandomNumberGenerator.GetInt32);
    }

    public class TestHarnessBuilder
    {
        private readonly List<PrizeDescription> _prizeDescriptions = [];
        private List<Driver> _drivers = [];
        private IDictionary<string, string> _driverToCarMap = new Dictionary<string, string>();
        private readonly Dictionary<string, IDictionary<string, bool>> _carToStickerMap = new();
        private readonly Dictionary<string, string> _carRentalMap = new();

        private readonly IList<PrizeWinner> _previousWinners = new List<PrizeWinner>();

        private readonly Random _random = new(1337);

        private static readonly string[] MostSponsorNames = ["_425", "AAF", "Alpinestars", "Bimmerworld", "Griots", "Redline", "RoR"];

        private static readonly PrizeDescription ToyoPrize = new()
        {
            sponsorName = SponsorConstants.ToyoTires,
            description = "4 toyo tires",
            sku = "1",
            serial = "0"
        };

        private readonly Faker<Driver> _driverFaker = new Faker<Driver>()
            .RuleFor(a => a.name, f => f.Name.FullName())
            .RuleFor(a => a.carNumber, f => f.Random.Number(1, 100).ToString());

        private readonly Faker<PrizeDescription> _prizeFaker = new Faker<PrizeDescription>()
            .RuleFor(a => a.sponsorName, f => f.PickRandom(MostSponsorNames))
            .RuleFor(a => a.description, f => f.Lorem.Word())
            .RuleFor(a => a.sku, f => f.Random.Number(1, 100).ToString())
            .RuleFor(a => a.serial, f => f.Random.Number(1, 100).ToString());

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
                    candidate = new DrawingCandidate
                    {
                        carNumber = _drivers.First().carNumber,
                        name = _drivers.First().name,
                    },
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
                var candidate = _drivers[_random.Next(_drivers.Count)];
                _previousWinners.Add(new PrizeWinner
                {
                    candidate = new DrawingCandidate
                    {
                        name = candidate.name,
                        carNumber = candidate.carNumber,
                    },
                    prizeDescription = _prizeDescriptions[_random.Next(_prizeDescriptions.Count)],
                    resourceId = "nyan-counting"
                });
            }

            return this;
        }

        // public TestHarnessBuilder WithStickersAllStickers()
        // {
        //     foreach (var car in _driverToCarMap.Values)
        //     {
        //         _carToStickerMap[car] = new Dictionary<string, bool>();
        //         foreach (var sponsor in MostSponsorNames)
        //         {
        //             _carToStickerMap[car][sponsor] = allStickersMapped || _random.Next(1) != 1;
        //         }
        //         
        //         _carToStickerMap[car][ToyoPrize.sponsorName] = allStickersMapped || _random.Next(1) != 1;
        //
        //     }
        // }
        public TestHarnessBuilder WithStickers(bool allCarsMapped = true, bool allStickersMapped = true)
        {
            foreach (var car in _driverToCarMap.Values)
            {
                _carToStickerMap[car] = new Dictionary<string, bool>();
                foreach (var sponsor in MostSponsorNames)
                {
                    _carToStickerMap[car][sponsor.ToLower()] = allStickersMapped || _random.Next(1) != 1;
                }

                _carToStickerMap[car][ToyoPrize.sponsorName.ToLower()] = allStickersMapped || _random.Next(1) != 1;
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

        public TestHarnessBuilder WithRentals()
        {
            foreach (var car in _driverToCarMap.Values)
            {
                _carRentalMap[car] = CarOwner;
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
                CarToStickerMap = _carToStickerMap,
                CarRentalMap = _carRentalMap,
            };
        }
    }

    public record TestHarness
    {
        public required IList<PrizeDescription> PrizeDescriptions;
        public required IList<Driver> Drivers;
        public required IList<PrizeWinner> PreviousWinners;
        public required IDictionary<string, IDictionary<string, bool>> CarToStickerMap;
        public required IDictionary<string, string> CarRentalMap;

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

        public IList<DrawingCandidate> DriversAsCandidates(bool allowRentalsToWin)
        {
            var stickerManager = GetStickerManager(allowRentalsToWin);
            return Drivers.Select(driver => new DrawingCandidate
            {
                name = stickerManager.GetCandidateNameForCar(driver.carNumber, driver.name),
                carNumber = driver.carNumber,
            }).ToList();
        }

        public IStickerManager GetStickerManager(bool allowRentalsToWin)
        {
            return new InMemoryStickerManager(
                new LoggerFactory().CreateLogger<InMemoryStickerManager>(),
                new StickerParseResult
                {
                    carToStickerMapping = CarToStickerMap,
                    carRentalMap = CarRentalMap,
                    schemaVersion = "1.0"
                }, allowRentalsToWin);
        }

        public PrizeLimitChecker GetPrizeLimitChecker()
        {
            return new PrizeLimitChecker(this.PrizeDescriptions.Select(record => new PrizeDescriptionRecord
            {
                name = record.sponsorName,
                description = record.description,
                sku = record.sku,
                count = 1,
                seasonalLimit = 0
            }).ToList());
        }
    }
}