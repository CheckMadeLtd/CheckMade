namespace CheckMade.Common.Interfaces.ExternalServices.AzureServices;

public interface IBlobLoader
{
    public Task<string> UploadBlobAndGetLinkAsync(MemoryStream stream, string fileName);
    public Task<(MemoryStream, string)> DownloadBlobAsync(string blobUrl);
}