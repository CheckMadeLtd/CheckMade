using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> recentHistory);
}

internal class WorkflowIdentifier(
        IUserAuthWorkflow userAuthWorkflow,
        ILanguageSettingWorkflow languageSettingWorkflow,
        ILogoutWorkflow logoutWorkflow) 
    : IWorkflowIdentifier
{
    public Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> recentHistory)
    {
        if (!IsUserAuthenticated(recentHistory))
            return Option<IWorkflow>.Some(userAuthWorkflow);

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

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> recentHistory) => 
        recentHistory.Count != 0 && 
        recentHistory.Last().OriginatorRole.IsSome;
}