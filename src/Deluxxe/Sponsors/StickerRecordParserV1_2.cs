using System.Diagnostics;

namespace Deluxxe.Sponsors;

public class StickerRecordParserV1_2 : IStickerRecordParser
{
    private const int ExpectedRowLength = 15;

    public string GetHeader() => "Number,Owner,Listed Color,Email 1,Email 2,Is A Rental,_425,AAF,Bimmerworld,Griots,Redline,RoR,Toyo,Proformance,Alpinestars";

    public void Parse(Activity? rowActivity, string row, Dictionary<string, IDictionary<string, bool>> carToStickerMap, Dictionary<string, string> carRentalMap)
    {
        rowActivity?.SetTag("StickerParserVersion", "1.2");
        var values = row.Split(',');
        // Number,   Owner,    Listed Color, Email 1,    Email 2, Is A Rental, _425, AAF, Bimmerworld, Griots, Redline, RoR, Toyo, Proformance, Alpinestars
        // 1,       A. Driver, Black/Orange, a@mail.com,        , n,           y,    y,   y,           y,      y,       y,   y,    y,           y

        // 0.       1.         2.            3.          4.       5.           6.    7.   8.           9.      10.      11.  12.   13.          14.


        if (values.Length != ExpectedRowLength)
        {
            rowActivity?.SetStatus(ActivityStatusCode.Error);
            rowActivity?.AddTag("error", $"values length too short, length={values.Length}, expected length={ExpectedRowLength}");
            return;
        }

        var carNumber = values[0].Trim();
        var isRental = IStickerRecordParser.ToBool(values[5].Trim());
        var owner = values[1].Trim();

        if (string.IsNullOrEmpty(carNumber))
        {
            rowActivity?.SetStatus(ActivityStatusCode.Error);
            rowActivity?.AddTag("error", "carNumber missing");
            return;
        }

        rowActivity?.AddTag("carNumber", carNumber);

        if (isRental)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", "owner missing");
                return;
            }

            carRentalMap[carNumber] = owner;
        }

        if (!carToStickerMap.TryGetValue(carNumber, out var value))
        {
            value = new Dictionary<string, bool>();
            carToStickerMap.Add(carNumber, value);
        }


        value[SponsorConstants._425.ToLower()] = IStickerRecordParser.ToBool(values[6]);
        value[SponsorConstants.AAF.ToLower()] = IStickerRecordParser.ToBool(values[7]);
        value[SponsorConstants.Alpinestars.ToLower()] = IStickerRecordParser.ToBool(values[14]);
        value[SponsorConstants.Bimmerworld.ToLower()] = IStickerRecordParser.ToBool(values[8]);
        value[SponsorConstants.Griots.ToLower()] = IStickerRecordParser.ToBool(values[9]);
        value[SponsorConstants.Proformance.ToLower()] = IStickerRecordParser.ToBool(values[13]);
        value[SponsorConstants.RoR.ToLower()] = IStickerRecordParser.ToBool(values[11]);
        value[SponsorConstants.Redline.ToLower()] = IStickerRecordParser.ToBool(values[10]);
        value[SponsorConstants.ToyoTires.ToLower()] = IStickerRecordParser.ToBool(values[12]);
    }
}