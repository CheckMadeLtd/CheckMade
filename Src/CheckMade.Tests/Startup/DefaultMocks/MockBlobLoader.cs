using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Tests.Startup.DefaultMocks;

public class MockBlobLoader : IBlobLoader
{
    public Task<Uri> UploadBlobAndReturnUriOrThrowAsync(MemoryStream stream, string fileName)
    {
        return Task.FromResult(new Uri("https://www.gorin.de/fakeUri"));
    }

    public Task<(MemoryStream, string)> DownloadBlobOrThrowAsync(Uri blobUri)
    {
        return Task.FromResult((new MemoryStream(), "fakeFileName"));
    }
}