namespace CheckMade.Abstract.Domain.ServiceInterfaces.ExtAPIs.Utils;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}