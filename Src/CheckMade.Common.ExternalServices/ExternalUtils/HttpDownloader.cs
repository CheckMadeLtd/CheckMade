using CheckMade.Common.DomainModel.Interfaces.ExternalServices;

namespace CheckMade.Common.ExternalServices.ExternalUtils;

public class HttpDownloader(HttpClient httpClient) : IHttpDownloader
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