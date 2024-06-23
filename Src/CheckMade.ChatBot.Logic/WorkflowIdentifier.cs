using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Task<Option<IWorkflow>> IdentifyAsync(IReadOnlyCollection<TlgInput> recentHistory);
}

internal class WorkflowIdentifier(
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        IUserAuthWorkflow userAuthWorkflow,
        ILanguageSettingWorkflow languageSettingWorkflow,
        ILogoutWorkflow logoutWorkflow) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(IReadOnlyCollection<TlgInput> recentHistory)
    {
        if (!await IsUserAuthenticatedAsync(recentHistory))
        {
            return Option<IWorkflow>.Some(userAuthWorkflow);
        }

        return ILogicUtils.GetLastBotCommand(recentHistory).Match(
            cmd => cmd.Details.BotCommandEnumCode.GetValueOrThrow() switch
            {
                // the settings & logout  BotCommand codes are the same across all InteractionModes
                (int)OperationsBotCommands.Settings => Option<IWorkflow>.Some(languageSettingWorkflow),
                (int)OperationsBotCommands.Logout => Option<IWorkflow>.Some(logoutWorkflow),
                
                _ => Option<IWorkflow>.None()
            },
            Option<IWorkflow>.None);
    }

    private async Task<bool> IsUserAuthenticatedAsync(IReadOnlyCollection<TlgInput> recentHistory)
    {
        if (recentHistory.Count == 0)
            return false;
        
        return (await roleBindingsRepo.GetAllAsync())
               .FirstOrDefault(arb => 
                   arb.TlgAgent.ChatId == recentHistory.Last().TlgAgent.ChatId && 
                   arb.TlgAgent.Mode == recentHistory.Last().TlgAgent.Mode && 
                   arb.Status == DbRecordStatus.Active) 
               != null; 
    } 
}