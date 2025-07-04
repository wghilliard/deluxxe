using System.Diagnostics;
using System.Text;
using Deluxxe.Mail;
using Deluxxe.Raffles;
using Deluxxe.Sponsors;
using Microsoft.Extensions.Hosting;

namespace DeluxxeCli;

public class RenderEmailsCliWorker(
    ActivitySource activitySource,
    CompletionToken completionToken,
    RaffleRunConfiguration runConfiguration,
    IRaffleResultReader raffleResultReader,
    StickerProviderUriResolver stickerProvider,
    Renderer renderer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var stickerManager = await stickerProvider.GetStickerManager(runConfiguration.stickerMapUri, runConfiguration.stickerMapSchemaVersion);

        using var activity = activitySource.StartActivity(nameof(RenderEmailsCliWorker));
        var raffleResult = await raffleResultReader.ReadAsync(new Uri($"file://local/{runConfiguration.name}-results.json"), token);
        var text = await renderer.Render(raffleResult, new RepresentationCalculator(stickerManager).Calculate(raffleResult.drawings
            .SelectMany(drawing => drawing.winners)
            .Select(winner => new Driver()
            {
                carNumber = winner.candidate.carNumber,
                name = winner.candidate.name,
            })
            .ToList()));
        const string announcementHtmlFile = "announcement.html";
        if (File.Exists(announcementHtmlFile))
        {
            File.Delete(announcementHtmlFile);
        }

        await using var stream = new FileStream(announcementHtmlFile, FileMode.Create);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(text);
        completionToken.Complete();
    }
}