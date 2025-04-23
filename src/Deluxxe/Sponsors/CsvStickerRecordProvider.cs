using System.Diagnostics;
using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Sponsors;

public class CsvStickerRecordProvider(ActivitySource activitySource, ILogger<CsvStickerRecordProvider> logger) : IStickerRecordProvider
{
    public async Task<StickerParseResult> Get(Uri uri)
    {
        if (!uri.IsFile || !uri.LocalPath.EndsWith("csv"))
        {
            return default;
        }

        var fileHandle = FileUriParser.Parse(uri).First();

        await using Stream stickerStream = new FileStream(fileHandle!.FullName, FileMode.Open);
        using var reader = new StreamReader(stickerStream);
        return await ParseCsvAsync(reader);
    }

    public async Task<StickerParseResult> ParseCsvAsync(StreamReader reader)
    {
        using var activity = activitySource.StartActivity("parse-cars-csv");

        var carToStickerMap = new Dictionary<string, IDictionary<string, bool>>();
        var carRentalMap = new Dictionary<string, string>();

        var index = 0;

        while (!reader.EndOfStream)
        {
            var row = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(row))
            {
                index++;
                continue;
            }

            if (index == 0)
            {
                index++;
                continue;
            }

            using var rowActivity = activitySource.StartActivity("parse-cars-csv-row");
            rowActivity?.AddTag("rowIndex", index);
            if (row.Length == 0)
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", "row empty");
                continue;
            }

            var values = row.Split(',');
            // Number,Driver,IsRental,_425,AAF,Alpinestars,Bimmerworld,Griots,Proformance,RoR,Redline,Toyo,Comment
            // 0.     1.     2.   3.   4.  5.          6.          7.     8.          9.  10.     11.  12.

            // Number,Driver,IsRental,_425,AAF,Alpinestars,Bimmerworld,Griots,Proformance,RoR,Redline,Toyo,Comment
            // 
            if (values.Length != 13)
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", $"values length too short, length={values.Length}");
                continue;
            }

            var carNumber = values[0].Trim();
            var isRental = ToBool(values[2].Trim());
            var owner =  values[1].Trim();

            if (string.IsNullOrEmpty(carNumber))
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", "carNumber missing");
                continue;
            }
            
            if (isRental)
            {
                if (string.IsNullOrWhiteSpace(owner))
                {
                    rowActivity?.SetStatus(ActivityStatusCode.Error);
                    rowActivity?.AddTag("error", "owner missing");
                    rowActivity?.AddTag("carNumber", carNumber);
                    continue;
                }
                carRentalMap[carNumber] = owner;
            }

            if (!carToStickerMap.TryGetValue(carNumber, out var value))
            {
                value = new Dictionary<string, bool>();
                carToStickerMap.Add(carNumber, value);
            }

            value[SponsorConstants._425] = ToBool(values[3]);
            value[SponsorConstants.AAF] = ToBool(values[4]);
            value[SponsorConstants.Alpinestars] = ToBool(values[5]);
            value[SponsorConstants.Bimmerworld] = ToBool(values[6]);
            value[SponsorConstants.Griots] = ToBool(values[7]);
            value[SponsorConstants.Proformance] = ToBool(values[8]);
            value[SponsorConstants.RoR] = ToBool(values[9]);
            value[SponsorConstants.Redline] = ToBool(values[10]);
            value[SponsorConstants.ToyoTires] = ToBool(values[11]);
        }

        return new StickerParseResult()
        {
            carToStickerMapping = carToStickerMap,
            carRentalMap = carRentalMap
        };
    }

    private static bool ToBool(string? value)
    {
        return value is "y";
    }
}