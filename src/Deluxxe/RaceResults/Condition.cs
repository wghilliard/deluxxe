namespace Deluxxe.RaceResults;

public class Condition(string field, string expression)
{
    private readonly bool _mustBeEqual = !expression.StartsWith('!');
    private readonly string _conditionsValue = expression.StartsWith('!') ? expression[1..] : expression;

    public bool IsSatisfied(RaceResultRecord raceResultRecord)
    {
        var recordsValue = field switch
        {
            "status" => raceResultRecord.status,
            "resultClass" => raceResultRecord.resultClass,
            _ => throw new InvalidOperationException($"Unknown field {field}")
        };

        if (_mustBeEqual)
        {
            return recordsValue == _conditionsValue;
        }
        return recordsValue != _conditionsValue;
    }
}