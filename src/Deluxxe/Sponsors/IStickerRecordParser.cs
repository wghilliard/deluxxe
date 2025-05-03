using System.Diagnostics;

namespace Deluxxe.Sponsors;

public interface IStickerRecordParser
{
    public void Parse(Activity? rowActivity, string row, Dictionary<string, IDictionary<string, bool>> carToStickerMap, Dictionary<string, string> carRentalMap);

    public static bool ToBool(string? value)
    {
        return value is "y";
    }
}