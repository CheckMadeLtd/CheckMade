namespace CheckMade.Common.Domain.Interfaces.ExternalServices.Utils;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}