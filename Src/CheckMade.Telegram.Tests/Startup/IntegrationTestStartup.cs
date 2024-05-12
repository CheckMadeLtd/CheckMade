using CheckMade.Telegram.Function.Startup;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        var prdDbConnString = (Config.GetValue<string>("PG_PRD_DB_CONNSTRING")
                              ?? throw new ArgumentNullException())
            .Replace("MYSECRET", Config.GetValue<string>("ConnectionStrings:PRD-DB-PSW"));
        Services.AddSingleton<PrdDbConnStringProvider>(_ => new PrdDbConnStringProvider(prdDbConnString));
    }
}
