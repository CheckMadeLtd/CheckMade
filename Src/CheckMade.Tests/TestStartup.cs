using CheckMade.Chat.Logic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests;

[UsedImplicitly]
public record TestStartup : IDisposable, IAsyncDisposable
{
    internal ServiceProvider ServiceProvider { get; private set; }

    public TestStartup()
    {
        var services = new ServiceCollection();
        
        services.Add_MessagingLogic_Dependencies();
        
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