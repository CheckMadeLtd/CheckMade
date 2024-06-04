using Azure.Storage;
using Azure.Storage.Blobs;
using CheckMade.Common.ExternalServices.AzureServices;
using CheckMade.Common.ExternalServices.GoogleApi;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.ExternalServices;

public static class DependencyRegistration
{
    public static void Add_GoogleApi_Dependencies(this IServiceCollection services, string googleApiCredential)
    {
        services.AddScoped<GoogleAuth>(_ => new GoogleAuth(googleApiCredential));
        services.AddScoped<ISheetsService, GoogleSheetsService>();
    }

    public static void Add_AzureServices_Dependencies(
        this IServiceCollection services,
        string blobContainerUri, string blobContainerAccountName, string blobContainerAccountKey)
    {
        services.AddScoped<IBlobLoader, BlobLoader>();
        services.AddScoped<BlobContainerClient>(_ =>
            new BlobContainerClient(
                new Uri(blobContainerUri),
                new StorageSharedKeyCredential(blobContainerAccountName, blobContainerAccountKey)));
    }
}