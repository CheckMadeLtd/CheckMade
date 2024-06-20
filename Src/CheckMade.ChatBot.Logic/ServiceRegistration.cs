using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic;

public static class ServiceRegistration
{
    public static void Register_ChatBotLogic_Services(this IServiceCollection services)
    {
        services.AddScoped<IInputProcessorFactory, InputProcessorFactory>();
        services.AddScoped<IWorkflowIdentifier, WorkflowIdentifier>();
        services.AddScoped<IWorkflowUtils>(sp => WorkflowUtils.CreateAsync(
            sp.GetRequiredService<ITlgInputRepository>(), 
            sp.GetRequiredService<ITlgTlgAgentRoleRepository>())
            .Result);

        services.AddScoped<IUserAuthWorkflow, UserAuthWorkflow>();
        services.AddScoped<ILanguageSettingWorkflow, LanguageSettingWorkflow>();
    }
}