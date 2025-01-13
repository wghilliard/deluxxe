using System.Diagnostics;
using System.Net;
using Deluxxe.RaceResults;
using Moq;
using Moq.Protected;
using Xunit.Abstractions;

namespace Deluxxe.Tests.RaceResults;

public class TestSpeedHiveClient(ITestOutputHelper testOutputHelper)
{
    private static readonly ActivitySource Source = new("Deluxxe.Tests.RaceResults.TestSpeedHiveClient");

    [Fact]
    public async Task TestCsvParser_IsSuccess()
    {
        Stream stream = new FileStream(Path.Combine("TestData", "race-results.csv"), FileMode.Open);
        var client = new SpeedHiveClient(Source, new Mock<IHttpClientFactory>().Object);

        var results = (await client.ParseAsync(Task.FromResult(stream))).ToList();

        Assert.True(results.Count > 0);

        foreach (var result in results)
        {
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Position));
            Assert.False(string.IsNullOrEmpty(result.StartNumber));
            Assert.False(string.IsNullOrEmpty(result.Competitor));
            Assert.False(string.IsNullOrEmpty(result.Class));
            Assert.False(string.IsNullOrEmpty(result.TotalTime));
            Assert.False(string.IsNullOrEmpty(result.Diff));
            Assert.False(string.IsNullOrEmpty(result.Laps));
            Assert.False(string.IsNullOrEmpty(result.BestLap));
            Assert.False(string.IsNullOrEmpty(result.BestLapNumber));
            Assert.False(string.IsNullOrEmpty(result.BestSpeed));
        }
    }

    [Fact]
    public async Task TestHttpClient_IsSuccess()
    {
        Stream stream = new FileStream(Path.Combine("TestData", "race-results.csv"), FileMode.Open);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var client = new SpeedHiveClient(Source, mockFactory.Object);
        var results = (await client.GetResultsFromUrl(new Uri("http://localhost"))).ToList();
        Assert.True(results.Count > 0);
    }

    [Theory]
    [InlineData("https://speedhive.mylaps.com/sessions/8939619")]
    [InlineData("https://speedhive.mylaps.com/sessions/8939619?one=two")]
    public Task TestHttpClientUriTransform_IsSuccess(string input)
    {
        var output = SpeedHiveClient.GetApiUrlFromUiUrl(new Uri(input));

        Assert.Equal(new Uri("https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/sessions/8939619/csv"), output);
        return Task.CompletedTask;
    }
}