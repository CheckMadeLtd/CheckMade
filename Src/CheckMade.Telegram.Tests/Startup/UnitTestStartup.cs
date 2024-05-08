using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Startup.DefaultMocks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    public UnitTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.AddScoped<IMessageRepo, MockMessageRepo>(_ => new MockMessageRepo(new Mock<IMessageRepo>()));
        Services.AddSingleton<IBotClientFactory, MockBotClientFactory>(_ => new MockBotClientFactory());
    }
}