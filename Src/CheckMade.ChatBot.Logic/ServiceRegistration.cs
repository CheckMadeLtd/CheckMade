using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic;

public static class ServiceRegistration
{
    public static void Register_ChatBotLogic_Services(this IServiceCollection services)
    {
        services.AddSingleton<IDomainGlossary, DomainGlossary>();
        
        services.AddScoped<IInputProcessor, InputProcessor>();
        services.AddScoped<IWorkflowIdentifier, WorkflowIdentifier>();
        services.AddScoped<ILogicUtils, LogicUtils>();

        services.AddScoped<IUserAuthWorkflow, UserAuthWorkflow>();
        
        services.AddScoped<INewIssueWorkflow, NewIssueWorkflow>();
        services.AddScoped<INewIssueInitialTradeUnknown, NewIssueInitialTradeUnknown>();
        
        services.AddScoped<ILanguageSettingWorkflow, LanguageSettingWorkflow>();
        
        services.AddScoped<ILogoutWorkflow, LogoutWorkflow>();
    }
}