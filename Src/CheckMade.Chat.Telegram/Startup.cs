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
        var tgToken = config.GetValue<string>("TelegramBotConfiguration:SubmissionsBotToken");
    
        services.AddHttpClient("telegram_submissions_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        services.AddScoped<UpdateService>();
    }

    public static void ConfigurePersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        var dbConnectionString = hostingEnvironment switch
        {
            "Development" => config.GetConnectionString("DevDb") 
                             ?? throw new ArgumentNullException(nameof(hostingEnvironment),
                                 "Can't find dev db connstring"),
        
            "CI" => config.GetValue<string>("CI_DB_CONNSTRING") 
                    ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                        "Can't find ci db connstring"),
            
            "Production" => "",
                // (config.GetConnectionString("DevDb") 
                //     ?? throw new ArgumentNullException(nameof(config), "Can't find dev db connstring"))
                // .Replace("MYSECRET", 
                // config.GetValue<string>("ConnectionStrings:DevDbPsw") 
                // ?? throw new ArgumentNullException(nameof(config), 
                //     "Can't find dev db psw")),
            
            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };

        if (hostingEnvironment == "CI")
        {
            Console.Out.Write($"The connstring is: {dbConnectionString}");
        }
        
        services.Add_Persistence_Dependencies(dbConnectionString);
    }
    
    public static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.Add_MessagingLogic_Dependencies();
    }
}