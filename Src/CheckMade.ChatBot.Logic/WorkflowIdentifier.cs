using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssue;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory);
}

internal record WorkflowIdentifier(
        IUserAuthWorkflow UserAuthWorkflow,
        INewIssueWorkflow NewIssueWorkflow,
        ILanguageSettingWorkflow LanguageSettingWorkflow,
        ILogoutWorkflow LogoutWorkflow) 
    : IWorkflowIdentifier
{
    public Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<IWorkflow>.Some(UserAuthWorkflow);

        var lastBotCommand = inputHistory.GetLastBotCommand();
        
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
                            Option<IWorkflow>.Some(LanguageSettingWorkflow),
                        (int)OperationsBotCommands.Logout => 
                            Option<IWorkflow>.Some(LogoutWorkflow),
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
                        (int)OperationsBotCommands.NewIssue => Option<IWorkflow>.Some(NewIssueWorkflow),
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