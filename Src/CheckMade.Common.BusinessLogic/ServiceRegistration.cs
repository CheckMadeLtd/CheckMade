using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Model.Core.Trades.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.BusinessLogic;

public static class ServiceRegistration
{
    public static void Register_CommonBusinessLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter<SaniCleanTrade>, StakeholderReporter<SaniCleanTrade>>();
        services.AddScoped<IStakeholderReporter<SiteCleanTrade>, StakeholderReporter<SiteCleanTrade>>();
    }
}