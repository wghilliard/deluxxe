using Deluxxe.RaceResults;

namespace Deluxxe.Tests.RaceResults;

public class TestCondition
{
    [Theory]
    [InlineData("!DNS", "1:38", true)]
    [InlineData("!DNS", "DNS", false)]
    // the two cases below make no sense on their own, but are included for completeness
    [InlineData("DNS", "DNS", true)]
    [InlineData("DNS", "1:38", true)]
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
            status = "1:38",
            resultClass = "PRO3"
        };

        Assert.True(conditions.All(condition => condition.IsSatisfied(driver)));
    }
}