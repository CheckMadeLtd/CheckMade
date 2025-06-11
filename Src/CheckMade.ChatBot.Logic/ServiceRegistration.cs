using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;
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

        services.AddScoped<ISubmissionFactory<SanitaryTrade>, SubmissionFactory<SanitaryTrade>>();
        services.AddScoped<ISubmissionFactory<SiteCleanTrade>, SubmissionFactory<SiteCleanTrade>>();

        services.AddScoped<UserAuthWorkflow>();
        services.AddScoped<NewSubmissionWorkflow>();
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
        services.AddScoped<INewSubmissionCancelConfirmation<SanitaryTrade>, NewSubmissionCancelConfirmation<SanitaryTrade>>();
        services.AddScoped<INewSubmissionCancelConfirmation<SiteCleanTrade>, NewSubmissionCancelConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionCancelled<SanitaryTrade>, NewSubmissionCancelled<SanitaryTrade>>();
        services.AddScoped<INewSubmissionCancelled<SiteCleanTrade>, NewSubmissionCancelled<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SanitaryTrade>, NewSubmissionConsumablesSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SiteCleanTrade>, NewSubmissionConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionEditMenu<SanitaryTrade>, NewSubmissionEditMenu<SanitaryTrade>>();
        services.AddScoped<INewSubmissionEditMenu<SiteCleanTrade>, NewSubmissionEditMenu<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SanitaryTrade>, NewSubmissionEvidenceEntry<SanitaryTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SiteCleanTrade>, NewSubmissionEvidenceEntry<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SanitaryTrade>, NewSubmissionFacilitySelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SiteCleanTrade>, NewSubmissionFacilitySelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SanitaryTrade>, NewSubmissionAssessmentRating<SanitaryTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SiteCleanTrade>, NewSubmissionAssessmentRating<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionReview<SanitaryTrade>, NewSubmissionReview<SanitaryTrade>>();
        services.AddScoped<INewSubmissionReview<SiteCleanTrade>, NewSubmissionReview<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SanitaryTrade>, NewSubmissionSphereConfirmation<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SiteCleanTrade>, NewSubmissionSphereConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SanitaryTrade>, NewSubmissionSphereSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SiteCleanTrade>, NewSubmissionSphereSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SanitaryTrade>, NewSubmissionTypeSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SiteCleanTrade>, NewSubmissionTypeSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSucceeded<SanitaryTrade>, NewSubmissionSucceeded<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSucceeded<SiteCleanTrade>, NewSubmissionSucceeded<SiteCleanTrade>>();
    }
}