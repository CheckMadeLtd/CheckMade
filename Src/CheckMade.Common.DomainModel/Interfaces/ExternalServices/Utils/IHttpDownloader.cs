namespace CheckMade.Common.DomainModel.Interfaces.ExternalServices.Utils;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}