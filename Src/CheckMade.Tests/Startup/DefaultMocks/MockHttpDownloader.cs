using CheckMade.Common.Utils.Generic;

namespace CheckMade.Tests.Startup.DefaultMocks;

public class MockHttpDownloader : IHttpDownloader
{
    public Task<MemoryStream> DownloadDataOrThrowAsync(Uri fileUri)
    {
        return Task.FromResult(new MemoryStream());
    }
}