using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
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
        ILanguageSettingWorkflow languageSettingWorkflow) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(IReadOnlyCollection<TlgInput> recentHistory)
    {
        if (!await IsUserAuthenticatedAsync(recentHistory.Last().TlgAgent))
        {
            return Option<IWorkflow>.Some(userAuthWorkflow);
        }

        return ILogicUtils.GetLastBotCommand(recentHistory).Match(
            cmd => cmd.Details.BotCommandEnumCode.GetValueOrThrow() switch
            {
                // the settings BotCommand code is the same across all InteractionModes
                (int)OperationsBotCommands.Settings => Option<IWorkflow>.Some(languageSettingWorkflow),
                
                _ => Option<IWorkflow>.None()
            },
            Option<IWorkflow>.None);
    }
    
    private async Task<bool> IsUserAuthenticatedAsync(TlgAgent inputTlgAgent) => 
        (await roleBindingsRepo.GetAllAsync())
        .FirstOrDefault(arb => 
            arb.TlgAgent.ChatId == inputTlgAgent.ChatId && 
            arb.TlgAgent.Mode == inputTlgAgent.Mode && 
            arb.Status == DbRecordStatus.Active) 
        != null;
}