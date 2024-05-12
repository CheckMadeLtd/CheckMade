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

        /* Here not using the usual separation of connstring and psw and then '.Replace()' because this needs to
         also work on GitHub Actions Runner / CI Environment - Integration Tests that access the production db need
         to work there too. So to keep things simple we thus store the entire connstring incl
         the real password in secrets.json on 'Development' and in a single GitHub Repo Password on 'CI' */
        const string keyToPrdDbConnString = "FOR_INTEGRATION_TESTS__PG_PRD_DB_CONNSTRING";
        var prdDbConnString = Config.GetValue<string>(keyToPrdDbConnString)
                              ?? throw new InvalidOperationException($"Can't find {keyToPrdDbConnString}");
        
        Services.AddSingleton<PrdDbConnStringProvider>(_ => new PrdDbConnStringProvider(prdDbConnString));
    }
}
