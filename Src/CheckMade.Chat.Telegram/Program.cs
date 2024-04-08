using CheckMade.Interfaces;
using CheckMade.Chat.Logic;
using CheckMade.Chat.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var tgToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
              ?? throw new ArgumentException("Can not get token. Set token in environment setting");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddHttpClient("telegram_submissions_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        s.AddScoped<UpdateService>();
        s.Add_MessagingLogic_Dependencies();
    })
    .Build();

host.Run();