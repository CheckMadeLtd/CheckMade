namespace CheckMade.Abstract.Domain.Interfaces.ExternalServices.Utils;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}