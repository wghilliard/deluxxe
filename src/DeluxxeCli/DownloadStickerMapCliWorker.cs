using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deluxxe.Google;
using Deluxxe.IO;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class DownloadStickerMapCliWorker(
    ActivitySource activitySource,
    ILogger<DownloadStickerMapCliWorker> logger,
    CompletionToken completionToken,
    GoogleSheetService googleSheetService,
    IDirectoryManager directoryManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var activity = activitySource.StartActivity(nameof(DownloadStickerMapCliWorker));

        var confDir = directoryManager.configDir;
        var carMappingConfigFilePath = Path.Combine(confDir.FullName, "car-mapping-config.json");
        var config = await JsonSerializer.DeserializeAsync<CarMappingConfig>(new FileStream(carMappingConfigFilePath, FileMode.Open, FileAccess.Read), cancellationToken: token);
        if (config is null)
        {
            logger.LogError("Could not deserialize config file at {carMappingConfigFilePath}", carMappingConfigFilePath);
            completionToken.Complete();
            return;
        }

        var tokenPath = Path.Combine(confDir.FullName, "token.json");
        var credPath = Path.Combine(confDir.FullName, config.GoogleCredentialsFile);
        var sheetsService = await googleSheetService.AuthenticateAsync(credPath, tokenPath);
        var values = await googleSheetService.DownloadSheetDataAsync(sheetsService, config.SpreadsheetId, config.RangeName);

        if (values == null || values.Count == 0)
        {
            logger.LogInformation("No data found in the sheet.");
            completionToken.Complete();
            return;
        }

        var headers = values[0].Select(h => h.ToString()).ToList();
        var processedData = new List<Dictionary<string, string?>>();

        for (var i = 1; i < values.Count; i++)
        {
            var row = values[i];
            var processedRow = new Dictionary<string, string?>();
            foreach (var (ourColumn, sheetColumn) in config.ColumnMapping)
            {
                var index = headers.IndexOf(sheetColumn);
                if (index != -1 && index < row.Count)
                {
                    processedRow[ourColumn] = row[index]?.ToString()?.Trim();
                }
                else
                {
                    processedRow[ourColumn] = string.Empty;
                }
            }

            processedData.Add(processedRow);
        }

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        var outputFileName = Path.Combine(directoryManager.outputDir.FullName, $"car-to-sticker-mapping-{date}.csv");
        await using var writer = new StreamWriter(outputFileName, false, Encoding.UTF8);
        await writer.WriteLineAsync(string.Join(",", config.OutputColumns));

        foreach (var row in processedData)
        {
            var line = string.Join(",", config.OutputColumns.Select(c => row.GetValueOrDefault(c)));
            await writer.WriteLineAsync(line);
        }

        logger.LogInformation("Successfully downloaded and processed {rowCount} rows to {outputFile}", processedData.Count, outputFileName);

        completionToken.Complete();
    }
}