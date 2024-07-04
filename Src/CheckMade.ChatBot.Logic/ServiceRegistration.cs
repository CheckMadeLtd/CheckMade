using CheckMade.ChatBot.Logic.Workflows.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic;

public static class ServiceRegistration
{
    public static void Register_ChatBotLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IInputProcessor, InputProcessor>();
        services.AddScoped<IWorkflowIdentifier, WorkflowIdentifier>();
        services.AddScoped<ILogicUtils, LogicUtils>();

        services.AddScoped<IUserAuthWorkflow, UserAuthWorkflow>();
        services.AddScoped<INewIssueWorkflow, NewIssueWorkflow>();
        services.AddScoped<ILanguageSettingWorkflow, LanguageSettingWorkflow>();
        services.AddScoped<ILogoutWorkflow, LogoutWorkflow>();
    }
}