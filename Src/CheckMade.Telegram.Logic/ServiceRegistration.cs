using CheckMade.Telegram.Logic.UpdateProcessors;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Logic;

public static class ServiceRegistration
{
    public static void Register_TelegramLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IOperationsUpdateProcessor, OperationsUpdateProcessor>();
        services.AddScoped<ICommunicationsUpdateProcessor, CommunicationsUpdateProcessor>();
        services.AddScoped<INotificationsUpdateProcessor, NotificationsUpdateProcessor>();
        
        services.AddScoped<IUpdateProcessorSelector, UpdateProcessorSelector>();
    }
}