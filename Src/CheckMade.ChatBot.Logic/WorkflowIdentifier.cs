using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using static CheckMade.Common.Model.ChatBot.Input.TlgInputType;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<TlgInput> inputHistory);
}

internal sealed record WorkflowIdentifier(
    UserAuthWorkflow UserAuthWorkflow,
    NewSubmissionWorkflow NewSubmissionWorkflow,
    LanguageSettingWorkflow LanguageSettingWorkflow,
    LogoutWorkflow LogoutWorkflow,
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    IDomainGlossary Glossary) : IWorkflowIdentifier
{
    public async Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<WorkflowBase>.Some(UserAuthWorkflow);
        
        var allBridges = 
            await WorkflowUtils.GetWorkflowBridgesOrNoneAsync(inputHistory.Last().LiveEventContext);
        
        var activeWorkflowLauncherOption = GetWorkflowLauncherOfLastActiveWorkflow();

        if (activeWorkflowLauncherOption.IsNone)
            return Option<WorkflowBase>.None();

        var activeWorkflowLauncher = activeWorkflowLauncherOption.GetValueOrThrow();
        
        return activeWorkflowLauncher switch
        {
            { InputType: CommandMessage } 
                when activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() >= 
                     BotCommandMenus.GlobalBotCommandsCodeThreshold_90 => 
                GetGlobalMenuWorkflow(activeWorkflowLauncher),
            
            { InputType: CommandMessage, TlgAgent.Mode: Operations } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    (int)OperationsBotCommands.NewIssue => Option<WorkflowBase>.Some(NewSubmissionWorkflow),
                    _ => Option<WorkflowBase>.None()
                },
            
            { InputType: CommandMessage, TlgAgent.Mode: Notifications } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },

            { InputType: CommandMessage, TlgAgent.Mode: Communications } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },
            
            _ => throw new InvalidOperationException(
                $"An input with these properties must not be an {nameof(activeWorkflowLauncher)}.")
        };

        Option<TlgInput> GetWorkflowLauncherOfLastActiveWorkflow()
        {
            var lastLauncher = 
                inputHistory.LastOrDefault(i => i.IsWorkflowLauncher(allBridges));
            
            if (lastLauncher is null)
                return Option<TlgInput>.None();
            
            var lastWorkflowHistory =
                inputHistory.GetLatestRecordsUpTo(i => 
                    i.TlgMessageId == lastLauncher.TlgMessageId &&
                    i.TlgDate == lastLauncher.TlgDate);
            
            var isWorkflowActive =
                !lastWorkflowHistory
                    .Any(i =>
                        i.ResultantState.IsSome &&
                        Glossary.GetDtType(i.ResultantState.GetValueOrThrow().InStateId)
                            .IsAssignableTo(typeof(IWorkflowStateTerminator)));
            
            return isWorkflowActive switch
            {
                true => lastLauncher,
                _ => Option<TlgInput>.None()
            };
        }
        
        Option<WorkflowBase> GetGlobalMenuWorkflow(TlgInput inputWithGlobalBotCommand)
        {
            var lastBotCommandCode = inputWithGlobalBotCommand.Details.BotCommandEnumCode.GetValueOrThrow();
            
            return lastBotCommandCode switch
            {
                (int)OperationsBotCommands.Settings => 
                    Option<WorkflowBase>.Some(LanguageSettingWorkflow),
                (int)OperationsBotCommands.Logout => 
                    Option<WorkflowBase>.Some(LogoutWorkflow),
                _ => 
                    throw new ArgumentOutOfRangeException(nameof(lastBotCommandCode), 
                        $"An unhandled BotCommand must not exist above the " +
                        $"'{nameof(BotCommandMenus.GlobalBotCommandsCodeThreshold_90)}'")
            };
        }
    }

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}