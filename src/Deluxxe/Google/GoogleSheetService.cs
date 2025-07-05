using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Google;

public class GoogleSheetService(
    ActivitySource activitySource,
    ILogger<GoogleSheetService> logger)
{
    public async Task<SheetsService> AuthenticateAsync(string credentialsPath, string tokenPath)
    {
        using var activity = activitySource.StartActivity(nameof(AuthenticateAsync));
        logger.LogInformation("Authenticating with Google Sheets API using credentials at {credentialsPath} and token at {tokenPath}", credentialsPath, tokenPath);

        UserCredential credential;
        await using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                ["https://www.googleapis.com/auth/spreadsheets.readonly"],
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));
        }

        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Deluxxe"
        });
    }

    public async Task<IList<IList<object>>> DownloadSheetDataAsync(SheetsService service, string spreadsheetId, string rangeName)
    {
        using var activity = activitySource.StartActivity(nameof(DownloadSheetDataAsync));
        logger.LogInformation("Downloading data from spreadsheet {spreadsheetId} and range {rangeName}", spreadsheetId, rangeName);

        var request = service.Spreadsheets.Values.Get(spreadsheetId, rangeName);
        var response = await request.ExecuteAsync();
        return response.Values;
    }
}
