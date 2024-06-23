using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Option<IWorkflow> Identify(TlgInput input, IReadOnlyCollection<TlgInput> recentHistory);
}

internal class WorkflowIdentifier(
        ILogicUtils logicUtils,
        IUserAuthWorkflow userAuthWorkflow,
        ILanguageSettingWorkflow languageSettingWorkflow) 
    : IWorkflowIdentifier
{
    public Option<IWorkflow> Identify(TlgInput input, IReadOnlyCollection<TlgInput> recentHistory)
    {
        // ToDo: remove this as soon as Repo handles caching
        logicUtils.InitAsync();
        
        if (!IsUserAuthenticated(input.TlgAgent))
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
    
    private bool IsUserAuthenticated(TlgAgent inputTlgAgent) => 
        logicUtils.GetAllTlgAgentRoles()
        .FirstOrDefault(arb => 
            arb.TlgAgent.ChatId == inputTlgAgent.ChatId && 
            arb.TlgAgent.Mode == inputTlgAgent.Mode && 
            arb.Status == DbRecordStatus.Active) 
        != null;
}