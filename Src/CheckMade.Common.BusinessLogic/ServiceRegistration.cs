using CheckMade.Common.Interfaces.BusinessLogic;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.BusinessLogic;

public static class ServiceRegistration
{
    public static void Register_CommonBusinessLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter, StakeholderReporter>();
    }
}