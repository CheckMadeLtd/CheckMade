using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive.Notifications;
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

        services.AddScoped<IIssueFactory<SanitaryTrade>, IssueFactory<SanitaryTrade>>();
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
        services.AddScoped<INewIssueCancelConfirmation<SanitaryTrade>, NewIssueCancelConfirmation<SanitaryTrade>>();
        services.AddScoped<INewIssueCancelConfirmation<SiteCleanTrade>, NewIssueCancelConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewIssueCancelled<SanitaryTrade>, NewIssueCancelled<SanitaryTrade>>();
        services.AddScoped<INewIssueCancelled<SiteCleanTrade>, NewIssueCancelled<SiteCleanTrade>>();
        services.AddScoped<INewIssueConsumablesSelection<SanitaryTrade>, NewIssueConsumablesSelection<SanitaryTrade>>();
        services.AddScoped<INewIssueConsumablesSelection<SiteCleanTrade>, NewIssueConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueEditMenu<SanitaryTrade>, NewIssueEditMenu<SanitaryTrade>>();
        services.AddScoped<INewIssueEditMenu<SiteCleanTrade>, NewIssueEditMenu<SiteCleanTrade>>();
        services.AddScoped<INewIssueEvidenceEntry<SanitaryTrade>, NewIssueEvidenceEntry<SanitaryTrade>>();
        services.AddScoped<INewIssueEvidenceEntry<SiteCleanTrade>, NewIssueEvidenceEntry<SiteCleanTrade>>();
        services.AddScoped<INewIssueFacilitySelection<SanitaryTrade>, NewIssueFacilitySelection<SanitaryTrade>>();
        services.AddScoped<INewIssueFacilitySelection<SiteCleanTrade>, NewIssueFacilitySelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueAssessmentRating<SanitaryTrade>, NewIssueAssessmentRating<SanitaryTrade>>();
        services.AddScoped<INewIssueAssessmentRating<SiteCleanTrade>, NewIssueAssessmentRating<SiteCleanTrade>>();
        services.AddScoped<INewIssueReview<SanitaryTrade>, NewIssueReview<SanitaryTrade>>();
        services.AddScoped<INewIssueReview<SiteCleanTrade>, NewIssueReview<SiteCleanTrade>>();
        services.AddScoped<INewIssueSphereConfirmation<SanitaryTrade>, NewIssueSphereConfirmation<SanitaryTrade>>();
        services.AddScoped<INewIssueSphereConfirmation<SiteCleanTrade>, NewIssueSphereConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewIssueSphereSelection<SanitaryTrade>, NewIssueSphereSelection<SanitaryTrade>>();
        services.AddScoped<INewIssueSphereSelection<SiteCleanTrade>, NewIssueSphereSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueTypeSelection<SanitaryTrade>, NewIssueTypeSelection<SanitaryTrade>>();
        services.AddScoped<INewIssueTypeSelection<SiteCleanTrade>, NewIssueTypeSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueSubmissionSucceeded<SanitaryTrade>, NewIssueSubmissionSucceeded<SanitaryTrade>>();
        services.AddScoped<INewIssueSubmissionSucceeded<SiteCleanTrade>, NewIssueSubmissionSucceeded<SiteCleanTrade>>();
    }
}