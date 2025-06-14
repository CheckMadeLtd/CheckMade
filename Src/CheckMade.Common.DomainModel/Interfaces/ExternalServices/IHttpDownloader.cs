namespace CheckMade.Common.DomainModel.Interfaces.ExternalServices;

public interface IHttpDownloader
{
    Task<MemoryStream> DownloadDataAsync(Uri fileUri);
}