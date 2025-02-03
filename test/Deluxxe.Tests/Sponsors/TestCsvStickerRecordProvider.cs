using Deluxxe.Sponsors;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Deluxxe.Tests.Sponsors;

public class TestCsvStickerRecordProvider(ITestOutputHelper testOutputHelper) : BaseTest(testOutputHelper)
{
    [Fact]
    public async Task TestStickerRecordProvider_IsSuccessful()
    {
        var service = new CsvStickerRecordProvider(activitySource, loggerFactory.CreateLogger<CsvStickerRecordProvider>());

        await using var stream = new FileStream(Path.Combine("Data", "car-to-sticker-mapping.csv"), FileMode.Open);
        using var reader = new StreamReader(stream);
        var result = await service.ParseCsvAsync(reader);

        var cars = result.CarToStickerMapping.Values.ToList();
        Assert.True(cars.Count > 0);

        var pairs = cars.Select(pair => pair.GetEnumerator())
            .Aggregate(new List<KeyValuePair<string, bool>>(), (list, enumerator) =>
            {
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }

                enumerator.Dispose();

                return list;
            });

        Assert.Contains(pairs, pair => pair.Key == SponsorConstants._425);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.AAF);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.Alpinestars);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.Bimmerworld);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.Griots);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.Proformance);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.RoR);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.Redline);
        Assert.Contains(pairs, pair => pair.Key == SponsorConstants.ToyoTires);

        var values = pairs.Select(pair => pair.Value).ToList().Distinct();
        Assert.Equal(2, values.Count());
    }
}