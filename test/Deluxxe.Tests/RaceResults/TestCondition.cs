using Deluxxe.RaceResults;

namespace Deluxxe.Tests.RaceResults;

public class TestCondition
{
    [Theory]
    [InlineData("!DNS", "Normal", true)]
    [InlineData("!DNS", "DNF", true)]
    [InlineData("!DNS", "DNS", false)]
    public void TestEqualsOperator(string expression, string value, bool shouldPass)
    {
        var condition = new Condition("status", expression);

        Assert.Equal(shouldPass, condition.IsSatisfied(new RaceResultRecord()
        {
            name = "grayson",
            status = value,
            resultClass = "PRO3"
        }));
    }

    [Fact]
    public void TestMultipleConditions()
    {
        List<Condition> conditions =
        [
            new ("status", "!DNS"),
            new ("resultClass", "PRO3"),
        ];

        var driver = new RaceResultRecord()
        {
            name = "grayson",
            status = "Normal",
            resultClass = "PRO3"
        };

        Assert.True(conditions.All(condition => condition.IsSatisfied(driver)));
    }
    
    [Fact]
    public void TestMultipleConditions_ShouldFail()
    {
        List<Condition> conditions =
        [
            new ("status", "!DNS"),
            new ("resultClass", "PRO3"),
        ];

        var driver = new RaceResultRecord()
        {
            name = "grayson",
            status = "Normmal",
            resultClass = "SPM"
        };

        Assert.False(conditions.All(condition => condition.IsSatisfied(driver)));
    }
}