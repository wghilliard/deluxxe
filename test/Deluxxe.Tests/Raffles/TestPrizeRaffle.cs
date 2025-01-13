using System.Diagnostics;
using Bogus;
using Deluxxe.Extensions;
using Deluxxe.ModelsV3;
using Deluxxe.Raffles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit.Abstractions;

namespace Deluxxe.Tests.Raffles;

public class TestPrizeRaffle(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DrawPrize_DriverHasSticker()
    {
        using var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var winner = given.GetPrizeRaffle(testOutputHelper).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winner);
        testOutputHelper.WriteLine(winner.ToString());

        Assert.True(given.CarToStickerMap[winner.Driver.CarNumber][winner.PrizeDescription.SponsorName]);
    }

    [Fact]
    public void DrawPrize_DriverDoesNotHaveSticker()
    {
        using var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithNoStickers()
            .Build();

        var winner = given.GetPrizeRaffle(testOutputHelper).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.Null(winner);
    }

    [Fact]
    public void DrawPrize_DriverPreviousWon()
    {
        using var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(1, true)
            .WithStickers()
            .Build();

        var firstWinner = given.GetPrizeRaffle(testOutputHelper).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, given.PreviousWinners);
        Assert.NotNull(firstWinner);
        testOutputHelper.WriteLine(firstWinner.ToString());

        var previousWinners = new List<PrizeWinner<PrizeDescription>>();
        previousWinners.AddRange(given.PreviousWinners);
        previousWinners.Add(firstWinner);

        var secondWinner = given.GetPrizeRaffle(testOutputHelper).DrawPrize(given.PrizeDescriptions[0], given.RaceResults, previousWinners);
        Assert.Null(secondWinner);
    }

    [Fact]
    public void DrawPrizes_AllPrizesAwarded()
    {
        using var given = Given()
            .WithDrivers(2)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var (winners, notAwarded) = given.GetPrizeRaffle(testOutputHelper).DrawPrizes(given.PrizeDescriptions, given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winners);
        Assert.Equal(2, winners.Count);

        Assert.NotEqual(winners[0].Driver.Name, winners[1].Driver.Name);

        Assert.NotNull(notAwarded);
        Assert.Empty(notAwarded);
    }

    [Fact]
    public void DrawPrizes_MorePrizesThanDrivers()
    {
        using var given = Given()
            .WithDrivers(1)
            .WithPrizeDescriptions(2, true)
            .WithStickers()
            .Build();

        var (winners, notAwarded) = given.GetPrizeRaffle(testOutputHelper).DrawPrizes(given.PrizeDescriptions, given.RaceResults, given.PreviousWinners);
        Assert.NotNull(winners);
        Assert.Single(winners);

        Assert.NotNull(notAwarded);
        Assert.Single(notAwarded);
    }

    private static TestHarnessBuilder Given()
    {
        return new TestHarnessBuilder();
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
                PrizeDescriptions = _prizeDescriptions as IReadOnlyList<PrizeDescription>,
                Drivers = _drivers as IReadOnlyList<Driver>,
                RaceResults = _raceResults as IReadOnlyList<RaceResult>,
                PreviousWinners = _previousWinners as IReadOnlyList<PrizeWinner<PrizeDescription>>,
                DriverToCarMap = _driverToCarMap as IReadOnlyDictionary<string, string>,
                CarToStickerMap = _carToStickerMap
            };
        }
    }

    public record TestHarness : IDisposable
    {
        private static readonly ActivitySource Source = new("Deluxxe.Tests.Raffles.TestPrizeRaffle");
        private TracerProvider? _tracerProvider;
        
        public required IReadOnlyList<PrizeDescription> PrizeDescriptions;
        public required IReadOnlyList<Driver> Drivers;
        public required IReadOnlyList<RaceResult> RaceResults;
        public required IReadOnlyList<PrizeWinner<PrizeDescription>> PreviousWinners;
        public required IReadOnlyDictionary<string, string> DriverToCarMap;
        public required IDictionary<string, IDictionary<string, bool>> CarToStickerMap;

        public StickerManager GetStickerManager()
        {
            return new StickerManager(DriverToCarMap, CarToStickerMap);
        }

        public PrizeRaffle<PrizeDescription> GetPrizeRaffle(ITestOutputHelper testOutputHelper, bool shouldLogTraces = true)
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("RaffleService"))
                .AddSource("Deluxxe.Tests.Raffles.TestPrizeRaffle")
                .AddConsoleExporter(opts => opts.Targets = ConsoleExporterOutputTargets.Debug)
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri("http://localhost:4317");
                })
                .Build();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(logging =>
                {
                    // logging.AddConsoleExporter();
                    logging.AddOtlpExporter();
                });
                builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, XUnitLoggerProvider>());
                builder.Services.AddSingleton(testOutputHelper);
            });

            if (shouldLogTraces)
            {
                Trace.Listeners.Add(new LoggerTraceListener(loggerFactory));
            }

            return new PrizeRaffle<PrizeDescription>(loggerFactory.CreateLogger<PrizeRaffle<PrizeDescription>>(), Source, GetStickerManager());
        }

        public void Dispose()
        {
            _tracerProvider?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}