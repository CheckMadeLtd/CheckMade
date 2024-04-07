using CheckMade.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Messaging.Logic;

public static class DependencyRegistration
{
    public static void Add_MessagingLogic_Dependencies(this IServiceCollection services)
    {
        services.AddSingleton<IResponseGenerator, ResponseGenerator>();
    }
}