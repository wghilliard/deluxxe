using Deluxxe.Raffles;
using Deluxxe.Resources;
using Xunit.Abstractions;

namespace Deluxxe.Tests.Resources;

public class TestResourceIdBuilder(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestResourceIdBuilder_WorksForRace()
    {
        var resourceId = new ResourceIdBuilder()
            .WithSeason("2025")
            .WithEvent("spring-into-summer", "123")
            .WithRaceDrawingRound("saturday-group-1", "1234", "1")
            .WithPrize("toyo", "1")
            .WithSerial("1")
            .Build();

        Assert.Equal("season/2025/event/spring-into-summer/123/drawing/race/saturday-group-1/1234/round/1/prize/toyo/1/serial/1", resourceId);
    }

    [Fact]
    public void TestResourceIdBuilder_WorksForEvent()
    {
        var resourceId = new ResourceIdBuilder()
            .WithSeason("2025")
            .WithEvent("spring-into-summer", "123")
            .WithEventDrawingRound("1")
            .WithPrize("toyo", "1")
            .WithSerial("1")
            .Build();

        Assert.Equal("season/2025/event/spring-into-summer/123/drawing/event/round/1/prize/toyo/1/serial/1", resourceId);
    }

    [Fact]
    public void TestResourceIdBuilder_ThrowsWhenSeasonSetTwice()
    {
        Assert.Throws<ArgumentException>(() => new ResourceIdBuilder()
            .WithSeason("2025")
            .WithSeason("2026"));
    }

    [Fact]
    public void TestResourceIdBuilder_ThrowsWhenSeasonNotSet()
    {
        Assert.Throws<ArgumentException>(() => new ResourceIdBuilder()
            .WithEvent("spring-into-summer", "123"));
    }

    [Fact]
    public void TestResourceIdBuilder_Copy()
    {
        var builderOne = new ResourceIdBuilder()
            .WithSeason("2025")
            .WithEvent("spring-into-summer", "123")
            .WithRaceDrawingRound("saturday-group-1", "1234", "1");

        var builderTwo = builderOne.Copy();

        builderOne.WithPrize("toyo", "1");
        builderTwo.WithPrize("redline oil", "1");

        var resourceIdOne = builderOne.Build();
        var resourceIdTwo = builderTwo.Build();
        testOutputHelper.WriteLine(resourceIdOne);
        testOutputHelper.WriteLine(resourceIdTwo);
        Assert.NotEqual(resourceIdOne, resourceIdTwo);
    }

    [Fact]
    public void TestResourceIdBuilder_NormalizeEventName()
    {
        Assert.Equal("irdc-spring-into-summer", ResourceIdBuilder.NormalizeEventName("IRDC - Spring into Summer"));
    }
}