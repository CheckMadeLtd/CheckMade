using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
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

        services.AddScoped<IIssueFactory<SaniCleanTrade>, IssueFactory<SaniCleanTrade>>();
        services.AddScoped<IIssueFactory<SiteCleanTrade>, IssueFactory<SiteCleanTrade>>();

        services.AddScoped<UserAuthWorkflow>();
        services.AddScoped<NewIssueWorkflow>();
        services.AddScoped<LanguageSettingWorkflow>();
        services.AddScoped<LogoutWorkflow>();
        services.AddScoped<ViewAttachmentsWorkflow>();
        
        services.AddScoped<IOneStepWorkflowTerminator, OneStepWorkflowTerminator>();

        services.AddScoped<ILanguageSettingSelect, LanguageSettingSelect>();
        services.AddScoped<ILanguageSettingSet, LanguageSettingSet>();

        services.AddScoped<ILogoutWorkflowConfirm, LogoutWorkflowConfirm>();
        services.AddScoped<ILogoutWorkflowLoggedOut, LogoutWorkflowLoggedOut>();
        services.AddScoped<ILogoutWorkflowAborted, LogoutWorkflowAborted>();

        services.AddScoped<IUserAuthWorkflowTokenEntry, UserAuthWorkflowTokenEntry>();
        services.AddScoped<IUserAuthWorkflowAuthenticated, UserAuthWorkflowAuthenticated>();

        services.AddScoped<INewIssueTradeSelection, NewIssueTradeSelection>();
        services.AddScoped<INewIssueCancelConfirmation<SaniCleanTrade>, NewIssueCancelConfirmation<SaniCleanTrade>>();
        services.AddScoped<INewIssueCancelConfirmation<SiteCleanTrade>, NewIssueCancelConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewIssueCancelled<SaniCleanTrade>, NewIssueCancelled<SaniCleanTrade>>();
        services.AddScoped<INewIssueCancelled<SiteCleanTrade>, NewIssueCancelled<SiteCleanTrade>>();
        services.AddScoped<INewIssueConsumablesSelection<SaniCleanTrade>, NewIssueConsumablesSelection<SaniCleanTrade>>();
        services.AddScoped<INewIssueConsumablesSelection<SiteCleanTrade>, NewIssueConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueEditMenu<SaniCleanTrade>, NewIssueEditMenu<SaniCleanTrade>>();
        services.AddScoped<INewIssueEditMenu<SiteCleanTrade>, NewIssueEditMenu<SiteCleanTrade>>();
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
        services.AddScoped<INewIssueSubmissionSucceeded<SaniCleanTrade>, NewIssueSubmissionSucceeded<SaniCleanTrade>>();
        services.AddScoped<INewIssueSubmissionSucceeded<SiteCleanTrade>, NewIssueSubmissionSucceeded<SiteCleanTrade>>();
    }
}