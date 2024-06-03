using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Tests.Startup.DefaultMocks;

public class MockBlobLoader : IBlobLoader
{
    public Task<string> UploadBlobAndGetLinkAsync(MemoryStream stream, string fileName)
    {
        return Task.FromResult("https://www.gorin.de/fakeUri");
    }

    public Task<(MemoryStream, string)> DownloadBlobAsync(string blobUrl)
    {
        return Task.FromResult((new MemoryStream(), "fakeFileName"));
    }
}