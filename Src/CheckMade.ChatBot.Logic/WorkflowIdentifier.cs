using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory);
}

internal class WorkflowIdentifier(
        IUserAuthWorkflow userAuthWorkflow,
        INewIssueWorkflow newIssueWorkflow,
        ILanguageSettingWorkflow languageSettingWorkflow,
        ILogoutWorkflow logoutWorkflow) 
    : IWorkflowIdentifier
{
    public Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<IWorkflow>.Some(userAuthWorkflow);

        var lastBotCommand = ILogicUtils.GetLastBotCommand(inputHistory);
        
        return lastBotCommand.Match(
            cmd =>
            {
                var lastBotCommandCode = 
                    lastBotCommand.GetValueOrThrow().Details.BotCommandEnumCode.GetValueOrThrow();

                if (lastBotCommandCode >= BotCommandMenus.SameBotCommandSemanticsThreshold_90)
                {
                    return lastBotCommandCode switch
                    {
                        (int)OperationsBotCommands.Settings => 
                            Option<IWorkflow>.Some(languageSettingWorkflow),
                        (int)OperationsBotCommands.Logout => 
                            Option<IWorkflow>.Some(logoutWorkflow),
                        _ => 
                            throw new ArgumentOutOfRangeException(nameof(lastBotCommandCode), 
                            $"An unhandled BotCommand must not exist above the " +
                            $"'{nameof(BotCommandMenus.SameBotCommandSemanticsThreshold_90)}'")
                    };
                }
                
                return cmd.TlgAgent.Mode switch
                {
                    InteractionMode.Operations => lastBotCommandCode switch
                    {
                        (int)OperationsBotCommands.NewIssue => Option<IWorkflow>.Some(newIssueWorkflow),
                        _ => Option<IWorkflow>.None()
                    },

                    InteractionMode.Communications => lastBotCommandCode switch
                    {
                        _ => Option<IWorkflow>.None()
                    },

                    InteractionMode.Notifications => lastBotCommandCode switch
                    {
                        _ => Option<IWorkflow>.None()
                    },
                    
                    _ => throw new ArgumentOutOfRangeException(nameof(cmd.TlgAgent.Mode))
                };
            },
            Option<IWorkflow>.None);
    }

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}