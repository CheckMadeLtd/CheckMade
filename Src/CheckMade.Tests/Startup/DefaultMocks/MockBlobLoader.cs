using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Tests.Startup.DefaultMocks;

public class MockBlobLoader : IBlobLoader
{
    public Task<Uri> UploadBlobAndReturnUriAsync(MemoryStream stream, string fileName)
    {
        return Task.FromResult(new Uri("https://www.gorin.de/fakeUri"));
    }

    public Task<(MemoryStream, string)> DownloadBlobAsync(Uri blobUri)
    {
        return Task.FromResult((new MemoryStream(), "fakeFileName"));
    }
}