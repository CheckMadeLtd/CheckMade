using CheckMade.Telegram.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Persistence;

public static class DependencyRegistration
{
    public static void Add_TelegramPersistence_Dependencies(this IServiceCollection services)
    {
        services.AddScoped<ITelegramMessageRepo, TelegramMessageRepo>();
    }
}
