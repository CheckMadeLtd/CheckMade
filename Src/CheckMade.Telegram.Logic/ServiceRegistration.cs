using CheckMade.Telegram.Logic.InputProcessors;
using CheckMade.Telegram.Logic.InputProcessors.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Logic;

public static class ServiceRegistration
{
    public static void Register_TelegramLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IOperationsInputProcessor, OperationsInputProcessor>();
        services.AddScoped<ICommunicationsInputProcessor, CommunicationsInputProcessor>();
        services.AddScoped<INotificationsInputProcessor, NotificationsInputProcessor>();
        
        services.AddScoped<IInputProcessorSelector, InputProcessorSelector>();
    }
}