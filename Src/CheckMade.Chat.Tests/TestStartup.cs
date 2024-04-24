using CheckMade.Chat.Logic;
using CheckMade.Common.Persistence;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests;

// ToDo: Use the existing D.I. setup from the main app for tests too, with modifications. As I did on my first attempt.

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