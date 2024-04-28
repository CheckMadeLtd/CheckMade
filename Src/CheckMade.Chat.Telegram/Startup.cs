using CheckMade.Chat.Logic;
using CheckMade.Common.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace CheckMade.Chat.Telegram;

public static class Startup
{
    public static void ConfigureAppServices(this IServiceCollection services, IConfiguration config)
    {
        // In the Unix Env. the var names/keys need to use '_' but in dotnet / Azure Keyvault they need to use '-'
        
        var tgToken = config.GetValue<string>("CHECKMADE-SUBMISSIONS-BOT-TOKEN");
    
        services.AddHttpClient("CheckMadeSubmissionsBot")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        services.AddScoped<UpdateService>();
    }

    public static void ConfigurePersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        var dbConnectionString = hostingEnvironment switch
        {
            "Development" or "CI" => 
                config.GetValue<string>("PG_DB_CONNSTRING") 
                ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                    "Can't find DEV_OR_CI_DB_CONNSTRING"),
            
            "Production" or "Staging" => 
                (Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_PRD_DB") 
                 ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                     "Can't find POSTGRESQLCONNSTR_PRD_DB"))
                .Replace("MYSECRET", config.GetValue<string>("ConnectionStrings:PRD_DB_PSW") 
                                     ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                                         "Can't find ConnectionStrings:PRD_DB_PSW")),
            
            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };

        services.Add_Persistence_Dependencies(dbConnectionString);
    }
    
    public static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.Add_MessagingLogic_Dependencies();
    }
}