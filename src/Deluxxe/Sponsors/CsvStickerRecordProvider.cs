using System.Diagnostics;
using Deluxxe.IO;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Sponsors;

public class CsvStickerRecordProvider(ActivitySource activitySource, ILogger<CsvStickerRecordProvider> logger) : IStickerRecordProvider
{
    public async Task<StickerParseResult> Get(Uri uri, string schemaVersion)
    {
        if (!uri.IsFile || !uri.LocalPath.EndsWith("csv"))
        {
            return default;
        }

        var fileHandle = FileUriParser.Parse(uri).First();

        await using Stream stickerStream = new FileStream(fileHandle!.FullName, FileMode.Open);
        using var reader = new StreamReader(stickerStream);
        return await ParseCsvAsync(reader, schemaVersion);
    }

    public async Task<StickerParseResult> ParseCsvAsync(StreamReader reader, string schemaVersion)
    {
        using var activity = activitySource.StartActivity("parse-cars-csv");

        var carToStickerMap = new Dictionary<string, IDictionary<string, bool>>();
        var carRentalMap = new Dictionary<string, string>();

        var index = 0;


        IStickerRecordParser recordParser = schemaVersion switch
        {
            "1.2" => new StickerRecordParserV1_2(),
            "1.0" => new StickerRecordParserV1_0(),
            _ => new StickerRecordParserV1_0()
        };

        while (!reader.EndOfStream)
        {
            var row = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(row))
            {
                index++;
                continue;
            }

            if (index == 0 && row.Trim() == recordParser.GetHeader())
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

            recordParser.Parse(rowActivity, row, carToStickerMap, carRentalMap);
        }

        return new StickerParseResult()
        {
            carToStickerMapping = carToStickerMap,
            carRentalMap = carRentalMap,
            schemaVersion = schemaVersion
        };
    }
}