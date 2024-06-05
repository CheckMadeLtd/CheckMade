namespace CheckMade.Common.Interfaces.ExternalServices.AzureServices;

public interface IBlobLoader
{
    public Task<Uri> UploadBlobAndReturnUriOrThrowAsync(MemoryStream stream, string fileName);
    public Task<(MemoryStream, string)> DownloadBlobOrThrowAsync(Uri blobUri);
}