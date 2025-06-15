using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.Logic;
using CheckMade.Common.Domain.Logic;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Domain;

public static class ServiceRegistration
{
    public static void Register_CommonBusinessLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter<SanitaryTrade>, StakeholderReporter<SanitaryTrade>>();
        services.AddScoped<IStakeholderReporter<SiteCleanTrade>, StakeholderReporter<SiteCleanTrade>>();
    }
}