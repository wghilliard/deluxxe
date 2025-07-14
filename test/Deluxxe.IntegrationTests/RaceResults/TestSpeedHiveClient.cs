using System.Diagnostics;
using Deluxxe.RaceResults;
using Deluxxe.RaceResults.SpeedHive;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Deluxxe.IntegrationTests.RaceResults;

public class TestSpeedHiveClient(ITestOutputHelper testOutputHelper)
{
    private static readonly ActivitySource Source = new("Deluxxe.Tests.RaceResults.TestSpeedHiveClient");

    [Fact]
    [Trait("Category", "http")]
    public async Task TestHttpClient_IsSuccess()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClient();
        services.AddSingleton(Source);
        services.AddSingleton<SpeedHiveClient>();

        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<SpeedHiveClient>();
        var results = (await client.GetResultsFromJsonUrl(new Uri("https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/sessions/8939601/classification"))).rows.ToList();
        Assert.True(results.Count > 0);
    }
}