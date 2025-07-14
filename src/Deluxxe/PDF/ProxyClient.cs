using Microsoft.Extensions.Logging;

namespace Deluxxe.PDF;

public class ProxyClient(IHttpClientFactory clientFactory, ILogger<ProxyClient> logger)
{
    private static readonly Uri ProxyUrl = new("http://localhost:3001");

    public async Task UploadAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        await clientFactory.CreateClient().PostAsync(new Uri(ProxyUrl, fileName), new StringContent(content), cancellationToken);
    }

    public async Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var response = await clientFactory.CreateClient().DeleteAsync(new Uri(ProxyUrl, fileName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        logger.LogWarning("Failed to delete file {FileName} from proxy: {StatusCode}", fileName, response.StatusCode);
        return false;
    }

    public async Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var response = await clientFactory.CreateClient().GetAsync(new Uri(ProxyUrl, fileName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        logger.LogWarning("File {FileName} does not exist on proxy: {StatusCode}", fileName, response.StatusCode);
        return false;
    }
}