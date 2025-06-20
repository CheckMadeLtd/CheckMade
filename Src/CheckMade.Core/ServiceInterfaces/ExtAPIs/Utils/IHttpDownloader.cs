namespace CheckMade.Core.ServiceInterfaces.ExtAPIs.Utils;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}