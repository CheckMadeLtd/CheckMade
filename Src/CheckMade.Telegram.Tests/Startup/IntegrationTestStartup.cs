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

        const string keyToPrdDbConnString = "PG_PRD_DB_CONNSTRING";
        const string keyToPrdDbPsw = "ConnectionStrings:PRD-DB-PSW";
        
        var prdDbConnString = (Config.GetValue<string>(keyToPrdDbConnString)
                              ?? throw new InvalidOperationException($"Can't find {keyToPrdDbConnString}"))
            .Replace(ConfigureServicesExtensions.PswPlaceholderString, 
                Config.GetValue<string>(keyToPrdDbPsw) 
                ?? throw new InvalidOperationException( $"Can't find {keyToPrdDbPsw}"));
        
        Services.AddSingleton<PrdDbConnStringProvider>(_ => new PrdDbConnStringProvider(prdDbConnString));
    }
}
