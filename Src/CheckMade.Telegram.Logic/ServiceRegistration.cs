using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Logic;

public static class ServiceRegistration
{
    public static void Register_TelegramLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IInputProcessorFactory, InputProcessorFactory>();
    }
}