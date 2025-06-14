using Azure.Storage;
using Azure.Storage.Blobs;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices.GoogleApi;
using CheckMade.Common.ExternalServices.AzureServices;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.ExternalServices.GoogleApi;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.ExternalServices;

public static class ServiceRegistration
{
    public static void Register_GoogleApi_Services(this IServiceCollection services, string googleApiCredential)
    {
        services.AddScoped<GoogleAuth>(_ => new GoogleAuth(googleApiCredential));
        services.AddScoped<ISheetsService, GoogleSheetsService>();
    }

    public static void Register_AzureServices_Services(
        this IServiceCollection services,
        string blobContainerUri, string blobContainerAccountName, string blobContainerAccountKey)
    {
        services.AddScoped<IBlobLoader, BlobLoader>();
        services.AddScoped<BlobContainerClient>(_ =>
            new BlobContainerClient(
                new Uri(blobContainerUri),
                new StorageSharedKeyCredential(blobContainerAccountName, blobContainerAccountKey)));
    }

    public static void Register_OtherExternalFacingServices_Services(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IHttpDownloader, HttpDownloader>();
    }
}