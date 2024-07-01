using CheckMade.Common.ExternalServices.ExternalUtils;

namespace CheckMade.Tests.Startup.DefaultStubs;

public class StubHttpDownloader : IHttpDownloader
{
    public Task<MemoryStream> DownloadDataAsync(Uri fileUri)
    {
        return Task.FromResult(new MemoryStream());
    }
}