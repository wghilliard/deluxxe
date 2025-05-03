using System.Diagnostics;

namespace Deluxxe.Sponsors;

public class StickerRecordParserV1_0 : IStickerRecordParser
{
    private const int ExpectedRowLength = 13;
    public void Parse(Activity? rowActivity, string row, Dictionary<string, IDictionary<string, bool>> carToStickerMap, Dictionary<string, string> carRentalMap)
    {
        rowActivity?.SetTag("StickerParserVersion", "1.0");
        var values = row.Split(',');
        // Number,Driver,IsRental,_425,AAF,Alpinestars,Bimmerworld,Griots,Proformance,RoR,Redline,Toyo,Comment
        // 0.     1.     2.   3.   4.  5.          6.          7.     8.          9.  10.     11.  12.

        // Number,Driver,IsRental,_425,AAF,Alpinestars,Bimmerworld,Griots,Proformance,RoR,Redline,Toyo,Comment
        // 
        if (values.Length != ExpectedRowLength)
        {
            rowActivity?.SetStatus(ActivityStatusCode.Error);
            rowActivity?.AddTag("error", $"values length too short, length={values.Length}, expected length={ExpectedRowLength}");
            return;
        }

        var carNumber = values[0].Trim();
        var isRental = IStickerRecordParser.ToBool(values[2].Trim());
        var owner = values[1].Trim();

        if (string.IsNullOrEmpty(carNumber))
        {
            rowActivity?.SetStatus(ActivityStatusCode.Error);
            rowActivity?.AddTag("error", "carNumber missing");
            return;
        }

        if (isRental)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", "owner missing");
                rowActivity?.AddTag("carNumber", carNumber);
                return;
            }

            carRentalMap[carNumber] = owner;
        }

        if (!carToStickerMap.TryGetValue(carNumber, out var value))
        {
            value = new Dictionary<string, bool>();
            carToStickerMap.Add(carNumber, value);
        }

        value[SponsorConstants._425] = IStickerRecordParser.ToBool(values[3]);
        value[SponsorConstants.AAF] = IStickerRecordParser.ToBool(values[4]);
        value[SponsorConstants.Alpinestars] = IStickerRecordParser.ToBool(values[5]);
        value[SponsorConstants.Bimmerworld] = IStickerRecordParser.ToBool(values[6]);
        value[SponsorConstants.Griots] = IStickerRecordParser.ToBool(values[7]);
        value[SponsorConstants.Proformance] = IStickerRecordParser.ToBool(values[8]);
        value[SponsorConstants.RoR] = IStickerRecordParser.ToBool(values[9]);
        value[SponsorConstants.Redline] = IStickerRecordParser.ToBool(values[10]);
        value[SponsorConstants.ToyoTires] = IStickerRecordParser.ToBool(values[11]);
    }
}