using CheckMade.Core.ServiceInterfaces.ExtAPIs.Utils;

namespace CheckMade.Tests.Startup.DefaultStubs;

public sealed class StubHttpDownloader : IHttpDownloader
{
    public Task<MemoryStream> DownloadDataAsync(Uri fileUri)
    {
        return Task.FromResult(new MemoryStream());
    }
}