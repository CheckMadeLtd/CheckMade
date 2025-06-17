using CheckMade.Abstract.Domain.Interfaces.ExternalServices.Utils;

namespace CheckMade.Common.ExternalServices.Utils;

public sealed class HttpDownloader(HttpClient httpClient) : IHttpDownloader
{
    public async Task<MemoryStream> DownloadDataAsync(Uri fileUri)
    {
        using (var response = await httpClient.GetAsync(fileUri))
        {
            response.EnsureSuccessStatusCode();

            var stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;

            return stream;
        }
    }
}