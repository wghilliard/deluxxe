using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Sponsors;

public class CsvStickerRecordProvider(ActivitySource activitySource, ILogger<CsvStickerRecordProvider> logger) : IStickerRecordProvider
{
    public async Task<StickerParseResult?> Get(Uri uri)
    {
        if (!uri.IsFile || !uri.LocalPath.EndsWith("csv"))
        {
            return null;
        }

        await using Stream stickerStream = new FileStream(uri.LocalPath, FileMode.Open);
        return await ParseCsvAsync(Task.FromResult(stickerStream));
    }

    public async Task<StickerParseResult> ParseCsvAsync(Task<Stream> input)
    {
        using var activity = activitySource.StartActivity("parse-cars-csv");
        using var reader = new StreamReader(input.Result);

        var carToStickerMap = new Dictionary<string, IDictionary<string, bool>>();

        var index = 0;

        foreach (var row in (await reader.ReadToEndAsync()).Trim().Split('\n'))
        {
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
            // Number,Driver,All?,_425,AAF,Alpinestars,Bimmerworld,Griots,Proformance,RoR,Redline,Toyo,Comment
            // 0.     1.     2.   3.   4.  5.          6.          7.     8.          9.  10.     11.  12.

            if (values.Length != 13)
            {
                rowActivity?.SetStatus(ActivityStatusCode.Error);
                rowActivity?.AddTag("error", $"values length too short, length={values.Length}");
                continue;
            }

            var carNumber = values[0].Trim();

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
            CarToStickerMapping = carToStickerMap
        };
    }

    private static bool ToBool(string? value)
    {
        return value is "y";
    }
}