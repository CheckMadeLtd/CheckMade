using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Common.ExternalServices.AzureServices;

public class BlobLoader : IBlobLoader
{
    public Task<string> UploadBlobAndGetLinkAsync(MemoryStream stream, string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<(MemoryStream, string)> DownloadBlobAsync(string blobUrl)
    {
        throw new NotImplementedException();
    }
}