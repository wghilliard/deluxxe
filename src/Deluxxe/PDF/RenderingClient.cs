using System.Diagnostics;
using Microsoft.Playwright;

namespace Deluxxe.PDF;

public class RenderingClient(ActivitySource activitySource, IPlaywright playwright)
{
    private const int Timeout = 5000;

    private readonly BrowserTypeConnectOptions _browserTypeConnectOptions = new()
    {
        Timeout = Timeout
    };

    public async Task<byte[]> GetResultsAsPdfAsync(Uri uiUrl, string locatorText, CancellationToken token = default)
    {
        using var activity = activitySource.StartActivity();
        activity?.AddTag("url", uiUrl.ToString());
        activity?.AddTag("timeout", Timeout);

        await using var browser = await playwright.Chromium.ConnectAsync("ws://localhost:3000/chromium/playwright", _browserTypeConnectOptions);

        activity?.AddEvent(new ActivityEvent("connected-to-browser"));

        var page = await browser.NewPageAsync();
        await page.SetViewportSizeAsync(1920, 1080);
        activity?.SetTag("viewport-width", 1920);
        activity?.SetTag("viewport-height", 1080);

        var response = await page.GotoAsync(uiUrl.ToString());
        activity?.AddEvent(new ActivityEvent("initial-page-loaded"));
        activity?.AddTag("response-status", response?.Status.ToString() ?? "unknown");

        var downloadButton = page.GetByText(locatorText);
        await downloadButton.WaitForAsync();
        activity?.AddEvent(new ActivityEvent("page-loaded"));

        var pdfBytes = await page.PdfAsync(new PagePdfOptions { PrintBackground = true });
        activity?.AddEvent(new ActivityEvent("pdf-rendered"));

        return pdfBytes;
    }
}