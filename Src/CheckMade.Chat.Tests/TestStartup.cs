using CheckMade.Chat.Telegram;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace CheckMade.Chat.Tests;

[UsedImplicitly]
public class TestStartup : IDisposable, IAsyncDisposable
{
    internal ServiceProvider ServiceProvider { get; }

    public TestStartup()
    {
        var services = new ServiceCollection();

        // ToDo: read environment for local test runs differently?? 

        // services.ConfigureServices(hostContext);
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}