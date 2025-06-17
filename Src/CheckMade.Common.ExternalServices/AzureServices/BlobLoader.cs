using Azure.Storage.Blobs;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Common.ExternalServices.AzureServices;

public sealed class BlobLoader(BlobContainerClient containerClient) : IBlobLoader
{
    private const string FileName = "FileName";
    
    public async Task<Uri> UploadBlobAndReturnUriAsync(MemoryStream stream, string fileName)
    {
        var blobName = Guid.NewGuid().ToString();
        var blobClient = containerClient.GetBlobClient(blobName);
        
        await blobClient.UploadAsync(stream);

        var metaData = new Dictionary<string, string> { { FileName, fileName } };
        await blobClient.SetMetadataAsync(metaData);

        return blobClient.Uri;
    }

    public async Task<(MemoryStream, string)> DownloadBlobAsync(Uri blobUri)
    {
        var blobName = Path.GetFileName(blobUri.LocalPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        var metaData = (await blobClient.GetPropertiesAsync()).Value.Metadata;
        var fileName = metaData[FileName];

        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);

        stream.Position = 0;
        return (stream, fileName);
    }
}