using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Logic.RequestProcessors.ByBotType;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Logic;

public static class DependencyRegistration
{
    public static void Add_TelegramLogic_Dependencies(this IServiceCollection services)
    {
        services.AddScoped<IOperationsRequestProcessor, OperationsRequestProcessor>();
        services.AddScoped<ICommunicationsRequestProcessor, CommunicationsRequestProcessor>();
        services.AddScoped<INotificationsRequestProcessor, NotificationsRequestProcessor>();
        
        services.AddScoped<IRequestProcessorSelector, RequestProcessorSelector>();
    }
}