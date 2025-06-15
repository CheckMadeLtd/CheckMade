using CheckMade.Common.DomainModel.Data.Core.Trades;
using CheckMade.Common.DomainModel.Interfaces.Logic;
using CheckMade.Common.DomainModel.Logic;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.DomainModel;

public static class ServiceRegistration
{
    public static void Register_CommonBusinessLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter<SanitaryTrade>, StakeholderReporter<SanitaryTrade>>();
        services.AddScoped<IStakeholderReporter<SiteCleanTrade>, StakeholderReporter<SiteCleanTrade>>();
    }
}