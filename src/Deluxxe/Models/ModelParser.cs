namespace Deluxxe.Models
{
    using System;
    using System.Linq;

    internal class ModelParser
    {
        public class Parsers
        {
            public static Car ParseCarFromLine(string line)
            {
                var values = line.Split(",").Select(v => v.Trim()).ToArray();
                return new Car
                {
                    Number = values[0],
                    DriverName = values[1],
                    HasAllStickers = Convert.ToBoolean(values[2]),
                    _425 = Convert.ToBoolean(values[3]),
                    AAF = Convert.ToBoolean(values[4]),
                    Alpinestars = Convert.ToBoolean(values[5]),
                    Bimmerworld = Convert.ToBoolean(values[6]),
                    Griots = Convert.ToBoolean(values[7]),
                    ProFormance = Convert.ToBoolean(values[8]),
                    RoR = Convert.ToBoolean(values[9]),
                    Redline = Convert.ToBoolean(values[10]),
                    Toyo = Convert.ToBoolean(values[11])
                };
            }

            public static PrizeDescriptor ParsePrizeDescriptorFromLine(string line)
            {
                var columns = line.Split(",").Select(v => v.Trim()).ToArray();
                return new PrizeDescriptor
                {
                    SponsorName = columns[0],
                    PrizeType = columns[1],
                    PerRace = int.Parse(columns[3]),
                    PerRaceCount = int.Parse(columns[4]),
                    Weekend = int.Parse(columns[5]),
                    PerWeekendCount = int.Parse(columns[6]),
                    PerWeekend = int.Parse(columns[7])
                };
            }

            public static RaceResult ParseRaceResultFromLine(string line, int raceId)
            {
                var columns = line.Split(",").Select(value => value.Trim().Trim('"')).ToArray();
                return new RaceResult
                {
                    DriverName = columns[0],
                    Position = int.Parse(columns[1]),
                    Number = columns[3],
                    RaceId = raceId,
                    CarClass = columns[4],
                    Gap = columns[5]
                };
            }
        }
    }
}