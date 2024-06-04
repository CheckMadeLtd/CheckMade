namespace CheckMade.Common.Utils.Generic;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataOrThrowAsync(Uri fileUri);
};

public class HttpDownloader(HttpClient httpClient) : IHttpDownloader
{
    public async Task<MemoryStream> DownloadDataOrThrowAsync(Uri fileUri)
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