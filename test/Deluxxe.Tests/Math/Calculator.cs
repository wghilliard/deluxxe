using Microsoft.Extensions.Logging;

namespace Deluxxe.Tests.Math;

public class Calculator(ILogger<Calculator> logger)
{

    public void ComputeVariance(IDictionary<string, int> results, int range)
    {
        double mean = 0;
        foreach (var (_, wins) in results)
        {
            mean += wins;
        }

        mean /= range;
        double variance = 0;

        foreach (var (_, wins) in results)
        {
            variance += System.Math.Pow(wins - mean, 2);
        }

        variance /= range - 1;

        logger.LogInformation($"mean: {mean}, variance: {variance}, standard deviation: {System.Math.Sqrt(variance)}");
        foreach (var (name, wins) in results)
        {
            logger.LogInformation($"{name}: {wins}");
        }
    }
}