using CheckMade.Telegram.Function.Startup;
using JetBrains.Annotations;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class IntegrationTestStartup : TestStartupBase
{
    public IntegrationTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.ConfigureBotClientServices(Config, HostingEnvironment);
        Services.ConfigurePersistenceServices(Config, HostingEnvironment);
    }
}
