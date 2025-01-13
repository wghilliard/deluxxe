namespace Deluxxe.RaceResults;

public record RaceResultRecord
{
    public string Position { get; set; }
    public string StartNumber { get; set; }
    public string Competitor { get; set; }
    public string Class { get; set; }
    public string TotalTime { get; set; }
    public string Diff { get; set; }
    public string Laps { get; set; }
    public string BestLap { get; set; }
    
    public string BestLapNumber { get; set; }
    public string BestSpeed { get; set; }
}