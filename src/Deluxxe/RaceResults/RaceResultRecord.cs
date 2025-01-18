namespace Deluxxe.RaceResults;

public record RaceResultRecord
{
    public string resultClass { get; set; } // aka car class
    public string status { get; set; } // Normal or DNS or DNF
    public string name { get; set; }
    public string startNumber { get; set; }     // aka Car Number
}

public record RaceResultResponse
{
    public IList<RaceResultRecord> rows { get; set; }
}