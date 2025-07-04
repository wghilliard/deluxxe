using Deluxxe.Raffles;

namespace Deluxxe.Sponsors;

public class RepresentationCalculator(IStickerManager stickerManager)
{
    public IDictionary<string, string> Calculate(IList<Driver> drivers)
    {
        var sponsorSums = new Dictionary<string, double>();
        var sponsorCounts = new Dictionary<string, int>();
        
        var output = new Dictionary<string, string>();
        
        foreach (var raceResult in drivers)
        {
            foreach (var sponsor in SponsorConstants.Sponsors)
            {
                var status = stickerManager.DriverHasSticker(raceResult.carNumber, sponsor);
                var val = status == StickerStatus.CarHasSticker ? 1.0 : 0.0;
                sponsorSums[sponsor] = sponsorSums.GetValueOrDefault(sponsor) + val;
                sponsorCounts[sponsor] = sponsorCounts.GetValueOrDefault(sponsor) + 1;
            }
        }
        
        foreach (var sponsor in SponsorConstants.Sponsors)
        {
            var stat = (sponsorSums[sponsor] / sponsorCounts[sponsor]) * 100;
            output[sponsor] = $"{stat:0.00}%";
        }

        return output;
    }
}