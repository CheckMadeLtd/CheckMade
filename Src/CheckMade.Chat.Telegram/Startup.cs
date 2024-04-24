using CheckMade.Chat.Logic;
using CheckMade.Common.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace CheckMade.Chat.Telegram;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services, HostBuilderContext hostContext)
    {
        var config = hostContext.Configuration;

        var tgToken = config.GetValue<string>("TelegramBotConfiguration:SubmissionsBotToken");
    
        services.AddHttpClient("telegram_submissions_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        services.AddScoped<UpdateService>();
        services.Add_MessagingLogic_Dependencies();

        var dbConnectionString = hostContext.HostingEnvironment.EnvironmentName switch
        {
            "Development" => 
                (config.GetConnectionString("DevDb") 
                 ?? throw new ArgumentNullException(nameof(hostContext), "Can't find dev db connstring"))
                .Replace("MYSECRET", 
                    config.GetValue<string>("ConnectionStrings:DevDbPsw") 
                    ?? throw new ArgumentNullException(nameof(hostContext), 
                        "Can't find dev db psw")),
        
            "TestsEnv" => "",
            
            "Production" => "",
        
            _ => throw new ArgumentException((nameof(hostContext.HostingEnvironment.EnvironmentName)))
        };
    
        services.Add_Persistence_Dependencies(dbConnectionString);
    }
}