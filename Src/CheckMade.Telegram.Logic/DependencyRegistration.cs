using CheckMade.Telegram.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Logic;

public static class DependencyRegistration
{
    public static void Add_TelegramLogic_Dependencies(this IServiceCollection services)
    {
        services.AddScoped<IRequestProcessor, RequestProcessor>();
    }
}