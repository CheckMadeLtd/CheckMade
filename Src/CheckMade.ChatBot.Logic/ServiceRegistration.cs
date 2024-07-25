using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic;

public static class ServiceRegistration
{
    public static void Register_ChatBotLogic_Services(this IServiceCollection services)
    {
        services.AddSingleton<IDomainGlossary, DomainGlossary>();
        
        services.AddScoped<IInputProcessor, InputProcessor>();
        services.AddScoped<IWorkflowIdentifier, WorkflowIdentifier>();
        services.AddScoped<IGeneralWorkflowUtils, GeneralWorkflowUtils>();
        services.AddScoped<IStateMediator, StateMediator>();

        services.AddScoped<IUserAuthWorkflow, UserAuthWorkflow>();
        services.AddScoped<INewIssueWorkflow, NewIssueWorkflow>();
        services.AddScoped<ILanguageSettingWorkflow, LanguageSettingWorkflow>();
        services.AddScoped<ILogoutWorkflow, LogoutWorkflow>();

        services.AddScoped<IIssueFactory, IssueFactory>();

        services.AddScoped<INewIssueTradeSelection, NewIssueTradeSelection>();
        services.AddScoped<INewIssueConsumablesSelection<SaniCleanTrade>, NewIssueConsumablesSelection<SaniCleanTrade>>();
        services.AddScoped<INewIssueConsumablesSelection<SiteCleanTrade>, NewIssueConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueEvidenceEntry<SaniCleanTrade>, NewIssueEvidenceEntry<SaniCleanTrade>>();
        services.AddScoped<INewIssueEvidenceEntry<SiteCleanTrade>, NewIssueEvidenceEntry<SiteCleanTrade>>();
        services.AddScoped<INewIssueFacilitySelection<SaniCleanTrade>, NewIssueFacilitySelection<SaniCleanTrade>>();
        services.AddScoped<INewIssueFacilitySelection<SiteCleanTrade>, NewIssueFacilitySelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueReview<SaniCleanTrade>, NewIssueReview<SaniCleanTrade>>();
        services.AddScoped<INewIssueReview<SiteCleanTrade>, NewIssueReview<SiteCleanTrade>>();
        services.AddScoped<INewIssueSphereConfirmation<SaniCleanTrade>, NewIssueSphereConfirmation<SaniCleanTrade>>();
        services.AddScoped<INewIssueSphereConfirmation<SiteCleanTrade>, NewIssueSphereConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewIssueSphereSelection<SaniCleanTrade>, NewIssueSphereSelection<SaniCleanTrade>>();
        services.AddScoped<INewIssueSphereSelection<SiteCleanTrade>, NewIssueSphereSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueTypeSelection<SaniCleanTrade>, NewIssueTypeSelection<SaniCleanTrade>>();
        services.AddScoped<INewIssueTypeSelection<SiteCleanTrade>, NewIssueTypeSelection<SiteCleanTrade>>();
    }
}