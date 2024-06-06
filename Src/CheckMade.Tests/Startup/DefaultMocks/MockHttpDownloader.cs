using CheckMade.Common.ExternalServices.ExternalUtils;

namespace CheckMade.Tests.Startup.DefaultMocks;

public class MockHttpDownloader : IHttpDownloader
{
    public Task<MemoryStream> DownloadDataAsync(Uri fileUri)
    {
        return Task.FromResult(new MemoryStream());
    }
}