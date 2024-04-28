using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Logic;

public static class DependencyRegistration
{
    public static void Add_MessagingLogic_Dependencies(this IServiceCollection services)
    {
        services.AddScoped<IRequestProcessor, RequestProcessor>();
    }
}