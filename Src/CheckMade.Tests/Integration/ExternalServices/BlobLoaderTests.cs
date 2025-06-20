using System.Text;
using CheckMade.Abstract.Domain.ServiceInterfaces.ExtAPIs.AzureServices;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.ExternalServices;

// ProblematicTestsOutsideOfIDE
public sealed class BlobLoaderTests
{
    private IServiceProvider? _services;

    [Fact]
    public async Task DownloadBlob_ReturnsExactSameBlob_WhenValidBlobIsUploaded()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var blobLoader = _services.GetRequiredService<IBlobLoader>();
        const string originalContent = "This is a test string plus some foreign characters: 这是一个测试行。";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(originalContent));
        const string fileName = "test_blob.txt";
        
        var blobUri = await blobLoader.UploadBlobAndReturnUriAsync(stream, fileName);
        var (downloadedStream, downloadedFileName) = await blobLoader.DownloadBlobAsync(blobUri);
        var downloadedContent = Encoding.UTF8.GetString(downloadedStream.ToArray());
        
        Assert.Equal(originalContent, downloadedContent);
        Assert.Equal(fileName, downloadedFileName);
    }
}