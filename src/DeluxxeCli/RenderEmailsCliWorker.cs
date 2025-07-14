using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deluxxe.Google;
using Deluxxe.IO;
using Deluxxe.Mail;
using Deluxxe.PDF;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeluxxeCli;

public class RenderEmailsCliWorker(
    ActivitySource activitySource,
    CompletionToken completionToken,
    RaffleRunConfiguration runConfiguration,
    IRaffleResultReader raffleResultReader,
    StickerProviderUriResolver stickerProvider,
    IDirectoryManager directoryManager,
    Renderer renderer,
    GoogleSheetService googleSheetService,
    RenderingClient renderingClient,
    ILogger<RenderEmailsCliWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var stickerManager = await stickerProvider.GetStickerManager(runConfiguration.stickerMapUri, runConfiguration.stickerMapSchemaVersion);

        using var activity = activitySource.StartActivity(nameof(RenderEmailsCliWorker));
        var raffleResult = await raffleResultReader.ReadCurrentContextAsync(token);
        var sponsorRepresentationTable = new RepresentationCalculator(stickerManager).Calculate(raffleResult.drawings
            .SelectMany(drawing => drawing.winners)
            .Select(winner => new Driver()
            {
                carNumber = winner.candidate.carNumber,
                name = winner.candidate.name,
            })
            .Distinct()
            .ToList());

        // announcement email
        var announcementText = await renderer.RenderAnnouncement(raffleResult, sponsorRepresentationTable);
        const string announcementHtmlFileName = "announcement-email.html";
        var announcementFile = Path.Combine(directoryManager.collateralDir.FullName, announcementHtmlFileName);
        await WriteToFileAsync(announcementFile, announcementText);

        // bimmerworld email

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

        if (values.Count == 0)
        {
            logger.LogInformation("No data found in the sheet.");
            completionToken.Complete();
            return;
        }

        var headers = values[0].Select(h => h.ToString()).ToList();
        int nameIndex = headers.IndexOf("Owner");
        int emailIndex = headers.IndexOf("Email 1");
        int altEmailIndex = headers.IndexOf("Email 2");

        var emailAddressMap = new Dictionary<string, string>();

        foreach (var row in values)
        {
            var name = row[nameIndex].ToString();
            var maybeManyNames = name!.Split('/');

            if (maybeManyNames.Length > 1)
            {
                emailAddressMap.TryAdd(maybeManyNames[0], row[emailIndex].ToString()!);
                emailAddressMap.TryAdd(maybeManyNames[1], row[altEmailIndex].ToString()!);
            }
            else
            {
                emailAddressMap.TryAdd(row[nameIndex].ToString()!, row[emailIndex].ToString()!);
            }
        }

        // bimmerworld
        var bimmerworldText = await renderer.RenderBimmerworld(raffleResult, emailAddressMap);
        const string bimmerworldHtmlFileName = "bimmerworld-email.html";
        var bimmerworldFile = Path.Combine(directoryManager.collateralDir.FullName, bimmerworldHtmlFileName);
        await WriteToFileAsync(bimmerworldFile, bimmerworldText);

        // aaf
        var aafText = await renderer.RenderAaf(raffleResult, emailAddressMap);
        const string aafHtmlFileName = "aaf-email.html";
        var aafFile = Path.Combine(directoryManager.collateralDir.FullName, aafHtmlFileName);
        await WriteToFileAsync(aafFile, aafText);

        // ror
        var rorText = await renderer.RenderRoR(raffleResult, emailAddressMap);
        const string rorHtmlFileName = "ror-email.html";
        var rorFile = Path.Combine(directoryManager.collateralDir.FullName, rorHtmlFileName);
        await WriteToFileAsync(rorFile, rorText);

        // toyo
        var toyoTextMap = await renderer.RenderDriverToyo(raffleResult);
        var toyoAwardMap = await renderer.RenderToyoAwardCollateral(raffleResult);
        foreach (var (shortName, text) in toyoTextMap)
        {
            var toyoFileName = $"{shortName}-toyo-email.html";
            var toyoFile = Path.Combine(directoryManager.collateralDir.FullName, toyoFileName);
            await WriteToFileAsync(toyoFile, text);

            var toyoAwardFileName = $"{shortName}-toyo-award.html";
            var toyoAwardFile = Path.Combine(directoryManager.collateralDir.FullName, toyoAwardFileName);
            await WriteToFileAsync(toyoAwardFile, toyoAwardMap[shortName]);

            var toyoAwardPdfFileName = $"{shortName}-toyo-award.pdf";
            var toyoAwardPdfFile = Path.Combine(directoryManager.collateralDir.FullName, toyoAwardPdfFileName);
            var toyoAwardPdf = await renderingClient.RenderContentAsPdfAsync(toyoAwardMap[shortName], token);
            await WriteToFileAsync(toyoAwardPdfFile, toyoAwardPdf);
        }

        completionToken.Complete();
    }

    private static async Task WriteToFileAsync(string fileName, string content)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(content);
    }

    private static async Task WriteToFileAsync(string fileName, byte[] content)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        // await using var writer = new Strea(stream);
        await stream.WriteAsync(content);
    }
}