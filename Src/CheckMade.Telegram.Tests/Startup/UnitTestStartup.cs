using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Mocks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase, IDisposable, IAsyncDisposable
{
    internal ServiceProvider ServiceProvider { get; set; } = null!;

    public UnitTestStartup()
    {
        ConfigureServices();
    }
    
    private new void ConfigureServices()
    {
        base.ConfigureServices();
        
        Services.AddScoped<IMessageRepo, MockMessageRepo>(_ => 
            new MockMessageRepo(new Mock<IMessageRepo>()));
        
        Services.AddSingleton<IBotClientFactory, MockBotClientFactory>(_ =>
            new MockBotClientFactory(new Mock<IBotClientFactory>()));
        
        ServiceProvider = Services.BuildServiceProvider();
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