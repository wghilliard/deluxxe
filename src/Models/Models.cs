namespace Deluxxe.Models
{
    public class Car
    {
        public string Number { get; set; }
        public string DriverName { get; set; }
        public bool HasAllStickers { get; set; }
        public bool _425 { get; set; }
        public bool AAF { get; set; }
        public bool Alpinestars { get; set; }
        public bool Bimmerworld { get; set; }
        public bool Griots { get; set; }
        public bool ProFormance { get; set; }
        public bool RoR { get; set; }
        public bool Redline { get; set; }
        public bool Toyo { get; set; }
    }

    public class CarNumber
    {
        string Value { get; set; }
    }


    public class RaceNumber
    {
        public int Value { get; set; }
    }

    public class RaceResult
    {
        public string DriverName { get; set; }
        public int Position { get; set; }
        public string Number { get; set; }
        public int RaceId { get; set; }
        public string CarClass { get; set; }
        public string Gap { get; set; }
    }


    public class RaceResults
    {
        public List<RaceResult> Drives { get; set; }
        public int Event { get; set; }
    }

    public class PrizeDescriptor
    {
        public string SponsorName { get; set; }
        public string PrizeType { get; set; }
        public int Races { get; set; }
        public int PerRace { get; set; }
        public int PerRaceCount { get; set; }
        public int Weekend { get; set; }
        public int PerWeekend { get; set; }
        public int PerWeekendCount { get; set; }
    }


    public class Prize
    {
        public string SponsorName { get; set; }
        public string PrizeType { get; set; }
        public string Frequency { get; set; }
        public int Amount { get; set; }
        public int RaceId { get; set; }

        public Prize(string sponsorName, string prizeType, string frequency, int amount, int raceId)
        {
            SponsorName = sponsorName;
            PrizeType = prizeType;
            Frequency = frequency;
            Amount = amount;
            RaceId = raceId;
        }
    }

    public class PrizeWinner
    {
        public string EventName { get; set; }
        public string PrizeType { get; set; }
        public int Id { get; set; }
        public string _425 { get; set; }
        public string AAF { get; set; }
        public string Alpinestars { get; set; }
        public string Bimmerworld { get; set; }
        public string Griots { get; set; }
        public string ProFormance { get; set; }
        public string RoR { get; set; }
        public string Redline { get; set; }
        public string Toyo { get; set; }
    }

    public class PrizeWinners
    {
        public int LastRace { get; set; }
        public Dictionary<int, List<PrizeWinner>> Winners { get; set; }

        public PrizeWinners(int lastRace, Dictionary<int, List<PrizeWinner>> winners)
        {
            LastRace = lastRace;
            Winners = winners;
        }
    }

    public class RoundResults
    {
        public List<PrizeAward> Awarded { get; set; }
        public List<Prize> Unawarded { get; set; }

        public RoundResults(List<PrizeAward> awarded, List<Prize> unawarded)
        {
            Awarded = awarded;
            Unawarded = unawarded;
        }
    }

    public class PrizeAward
    {
        public Prize Prize { get; set; }
        public RaceResult Winner { get; set; }
    }

    public class SponsorName
    {
        public string Value { get; set; }
    }

    public class WinnerSummary
    {
        public Dictionary<int, Dictionary<string, List<PrizeAward>>> ByRace { get; set; }
        public Dictionary<string, PrizeAward> Weekend { get; set; }
    }

    public class AwardRegistry
    {
        public List<EventRecord> Records { get; set; }
        public string LastUpdatedDateTime { get; set; }
        public string LastUpdatedBy { get; set; }

        public AwardRegistry(string lastUpdatedBy)
        {
            LastUpdatedBy = lastUpdatedBy;
            LastUpdatedDateTime = DateTime.UtcNow.ToString("o"); // ISO 8601 format
            Records = new List<EventRecord>();
        }
    }

    public class EventRecord
    {
        public string Name { get; set; }
        public List<RaceRecord> Races { get; set; }
        public List<Award> WeekendAwards { get; set; }
    }

    public class RaceRecord
    {
        public string Timestamp { get; set; }
        public int RaceId { get; set; }
        public List<Award> Awards { get; set; }
    }

    public class Award
    {
        public string DriverName { get; set; }
        public string SponsorName { get; set; }
        public string Value { get; set; }

        public Award(string driverName, string sponsorName, string value)
        {
            DriverName = driverName;
            SponsorName = sponsorName;
            Value = value;
        }
    }
}