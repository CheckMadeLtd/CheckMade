using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Common.ExternalServices.AzureServices;

public class BlobLoader : IBlobLoader
{
    public Task<Uri> UploadBlobAndReturnUriAsync(MemoryStream stream, string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<(MemoryStream, string)> DownloadBlobAsync(Uri blobUri)
    {
        throw new NotImplementedException();
    }
}