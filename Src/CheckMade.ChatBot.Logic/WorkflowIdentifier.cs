using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core.Trades.Concrete;
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
    NewIssueWorkflow NewIssueWorkflow,
    LanguageSettingWorkflow LanguageSettingWorkflow,
    LogoutWorkflow LogoutWorkflow,
    ViewAttachmentsWorkflow ViewAttachmentsWorkflow,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IStateMediator Mediator,
    IDomainGlossary Glossary) : IWorkflowIdentifier
{
    public async Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<WorkflowBase>.Some(UserAuthWorkflow);

        var currentLiveEvent = inputHistory.Last().LiveEventContext.GetValueOrThrow();
        var allBridges = await BridgesRepo.GetAllAsync(currentLiveEvent);
        
        var activeWorkflowLauncherOption = GetWorkflowLauncherOfLastActiveWorkflow();

        if (activeWorkflowLauncherOption.IsNone)
            return Option<WorkflowBase>.None();

        var activeWorkflowLauncher = activeWorkflowLauncherOption.GetValueOrThrow();
        
        return activeWorkflowLauncher switch
        {
            { InputType: CommandMessage } proactiveLauncher 
                when proactiveLauncher.Details.BotCommandEnumCode.GetValueOrThrow() >= 
                     BotCommandMenus.GlobalBotCommandsCodeThreshold_90 => GetGlobalMenuWorkflow(proactiveLauncher),
            
            { InputType: CommandMessage, TlgAgent.Mode: Operations } proactiveLauncher => 
                proactiveLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    (int)OperationsBotCommands.NewIssue => Option<WorkflowBase>.Some(NewIssueWorkflow),
                    _ => Option<WorkflowBase>.None()
                },
            
            { InputType: CommandMessage, TlgAgent.Mode: Notifications } proactiveLauncher => 
                proactiveLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },

            { InputType: CommandMessage, TlgAgent.Mode: Communications } proactiveLauncher => 
                proactiveLauncher.Details.BotCommandEnumCode.GetValueOrThrow() switch
                {
                    _ => Option<WorkflowBase>.None()
                },
            
            { InputType: CallbackQuery, TlgAgent.Mode: Notifications } reactiveLauncher =>
                GetReactiveWorkflowInNotificationsMode(reactiveLauncher),
                
            { InputType: CallbackQuery, TlgAgent.Mode: Communications } reactiveLauncher =>
                GetReactiveWorkflowInCommunicationsMode(reactiveLauncher),
            
            _ => throw new InvalidOperationException(
                $"An input with these properties must not be an {nameof(activeWorkflowLauncher)}.")
        };

        Option<TlgInput> GetWorkflowLauncherOfLastActiveWorkflow()
        {
            var lastLauncher = GetAllWorkflowLaunchers().LastOrDefault();
            
            if (lastLauncher is null)
                return Option<TlgInput>.None();
            
            // ToDo: replace with `i == lastBotCommand.GetValueOrThrow()` once I overloaded equals and == for TlgInput
            var lastWorkflowHistory =
                inputHistory.GetLatestRecordsUpTo(i => 
                    i.TlgMessageId == lastLauncher.TlgMessageId &&
                    i.TlgDate == lastLauncher.TlgDate);
            
            var isWorkflowActive =
                !lastWorkflowHistory
                    .Any(i =>
                        i.ResultantWorkflow.IsSome &&
                        Glossary.GetDtType(i.ResultantWorkflow.GetValueOrThrow().InStateId)
                            .IsAssignableTo(typeof(IWorkflowStateTerminator)));
            
            return isWorkflowActive switch
            {
                true => lastLauncher,
                _ => Option<TlgInput>.None()
            };
        }

        IReadOnlyCollection<TlgInput> GetAllWorkflowLaunchers()
        {
            return inputHistory
                .Where(i =>
                    i.InputType == CommandMessage ||
                    i.InputType == CallbackQuery &&
                    i.TlgAgent.Mode is Notifications or Communications &&
                    allBridges.Any(b =>
                        b.DestinationChatId == i.TlgAgent.ChatId &&
                        b.DestinationMessageId == i.TlgMessageId))
                .ToImmutableReadOnlyCollection();
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

        Option<WorkflowBase> GetReactiveWorkflowInNotificationsMode(TlgInput reactiveLauncher)
        {
            var sourceInput =
                allBridges.First(b =>
                        b.DestinationChatId == reactiveLauncher.TlgAgent.ChatId &&
                        b.DestinationMessageId == reactiveLauncher.TlgMessageId)
                    .SourceInput;

            var sourceWorkflowState = Mediator.Next(
                Glossary.GetDtType(
                    sourceInput.ResultantWorkflow.GetValueOrThrow().InStateId));

            return sourceWorkflowState switch
            {
                INewIssueReview<SaniCleanTrade> or INewIssueReview<SiteCleanTrade> =>
                    Option<WorkflowBase>.Some(ViewAttachmentsWorkflow),

                _ => throw new InvalidOperationException(
                    $"Unhandled {nameof(sourceWorkflowState)} while trying to identify reactive Workflow in" +
                    $"{nameof(Notifications)} mode")
            };
        }
        
        // ReSharper disable once UnusedParameter.Local
        Option<WorkflowBase> GetReactiveWorkflowInCommunicationsMode(TlgInput reactiveLauncher)
        {
            return Option<WorkflowBase>.None();
        }
    }

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}