using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.D_Terminators;
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
        
        services.AddScoped<ILanguageSettingSelect, LanguageSettingSelect>();
        services.AddScoped<ILanguageSettingSet, LanguageSettingSet>();

        services.AddScoped<ILogoutWorkflowConfirm, LogoutWorkflowConfirm>();
        services.AddScoped<ILogoutWorkflowLoggedOut, LogoutWorkflowLoggedOut>();
        services.AddScoped<ILogoutWorkflowAborted, LogoutWorkflowAborted>();

        services.AddScoped<IUserAuthWorkflowTokenEntry, UserAuthWorkflowTokenEntry>();
        services.AddScoped<IUserAuthWorkflowAuthenticated, UserAuthWorkflowAuthenticated>();

        services.AddScoped<INewSubmissionTradeSelection, NewSubmissionTradeSelection>();
        services.AddScoped<INewIssueCancelConfirmation<SanitaryTrade>, NewIssueCancelConfirmation<SanitaryTrade>>();
        services.AddScoped<INewIssueCancelConfirmation<SiteCleanTrade>, NewIssueCancelConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewIssueCancelled<SanitaryTrade>, NewIssueCancelled<SanitaryTrade>>();
        services.AddScoped<INewIssueCancelled<SiteCleanTrade>, NewIssueCancelled<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SanitaryTrade>, NewSubmissionConsumablesSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SiteCleanTrade>, NewSubmissionConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueEditMenu<SanitaryTrade>, NewIssueEditMenu<SanitaryTrade>>();
        services.AddScoped<INewIssueEditMenu<SiteCleanTrade>, NewIssueEditMenu<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SanitaryTrade>, NewSubmissionEvidenceEntry<SanitaryTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SiteCleanTrade>, NewSubmissionEvidenceEntry<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SanitaryTrade>, NewSubmissionFacilitySelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SiteCleanTrade>, NewSubmissionFacilitySelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SanitaryTrade>, NewSubmissionAssessmentRating<SanitaryTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SiteCleanTrade>, NewSubmissionAssessmentRating<SiteCleanTrade>>();
        services.AddScoped<INewIssueReview<SanitaryTrade>, NewIssueReview<SanitaryTrade>>();
        services.AddScoped<INewIssueReview<SiteCleanTrade>, NewIssueReview<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SanitaryTrade>, NewSubmissionSphereConfirmation<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SiteCleanTrade>, NewSubmissionSphereConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SanitaryTrade>, NewSubmissionSphereSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SiteCleanTrade>, NewSubmissionSphereSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SanitaryTrade>, NewSubmissionTypeSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SiteCleanTrade>, NewSubmissionTypeSelection<SiteCleanTrade>>();
        services.AddScoped<INewIssueSubmissionSucceeded<SanitaryTrade>, NewIssueSubmissionSucceeded<SanitaryTrade>>();
        services.AddScoped<INewIssueSubmissionSucceeded<SiteCleanTrade>, NewIssueSubmissionSucceeded<SiteCleanTrade>>();
    }
}