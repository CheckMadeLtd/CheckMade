using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Global.LanguageSetting;
using CheckMade.Bot.Workflows.Global.Logout;
using CheckMade.Bot.Workflows.Global.UserAuth;
using CheckMade.Bot.Workflows.Ops.NewSubmission;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Core.Model.Bot.Categories.InteractionMode;
using static CheckMade.Core.Model.Bot.Categories.InputType;

namespace CheckMade.Bot.Workflows;

public interface IWorkflowIdentifier
{
    Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<Input> inputHistory);
}

public sealed record WorkflowIdentifier(
    UserAuthWorkflow UserAuthWorkflow,
    NewSubmissionWorkflow NewSubmissionWorkflow,
    LanguageSettingWorkflow LanguageSettingWorkflow,
    LogoutWorkflow LogoutWorkflow,
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    IDomainGlossary Glossary) : IWorkflowIdentifier
{
    public async Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<Input> inputHistory)
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
            
            { InputType: CommandMessage, Agent.Mode: Operations } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    (int)OperationsBotCommands.NewSubmission => Option<WorkflowBase>.Some(NewSubmissionWorkflow),
                    _ => Option<WorkflowBase>.None()
                },
            
            { InputType: CommandMessage, Agent.Mode: Notifications } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },

            { InputType: CommandMessage, Agent.Mode: Communications } => 
                activeWorkflowLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },
            
            _ => throw new InvalidOperationException(
                $"An input with these properties must not be an {nameof(activeWorkflowLauncher)}.")
        };

        Option<Input> GetWorkflowLauncherOfLastActiveWorkflow()
        {
            var lastLauncher = 
                inputHistory.LastOrDefault(i => i.IsWorkflowLauncher(allBridges));
            
            if (lastLauncher is null)
                return Option<Input>.None();
            
            var lastWorkflowHistory =
                inputHistory.GetLatestRecordsUpTo(i => 
                    i.MessageId == lastLauncher.MessageId &&
                    i.TimeStamp == lastLauncher.TimeStamp);
            
            var isWorkflowActive =
                !lastWorkflowHistory
                    .Any(i =>
                        i.ResultantState.IsSome &&
                        Glossary.GetDtType(i.ResultantState.GetValueOrThrow().InStateId)
                            .IsAssignableTo(typeof(IWorkflowStateTerminator)));
            
            return isWorkflowActive switch
            {
                true => lastLauncher,
                _ => Option<Input>.None()
            };
        }
        
        Option<WorkflowBase> GetGlobalMenuWorkflow(Input inputWithGlobalBotCommand)
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

    private static bool IsUserAuthenticated(IReadOnlyCollection<Input> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}