using System.ComponentModel;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

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
    IDomainGlossary Glossary) 
    : IWorkflowIdentifier
{
    public async Task<Option<WorkflowBase>> IdentifyAsync(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<WorkflowBase>.Some(UserAuthWorkflow);

        var currentMode = inputHistory.Last().TlgAgent.Mode;

        return currentMode switch
        {
            InteractionMode.Operations => IdentifyOperationsWorkflow(),
            InteractionMode.Communications => IdentifyCommunicationsWorkflow(),
            InteractionMode.Notifications => await IdentifyNotificationsWorkflowAsync(),
            _ => throw new InvalidEnumArgumentException($"Unhandled {nameof(currentMode)}")
        };
        
        Option<WorkflowBase> IdentifyOperationsWorkflow()
        {
            return GetBotCommandOfLastActiveWorkflow().Match(
                cmd =>
                {
                    var lastBotCommandCode = cmd.Details.BotCommandEnumCode.GetValueOrThrow();

                    if (lastBotCommandCode >= BotCommandMenus.GlobalBotCommandsCodeThreshold_90)
                    {
                        return GetGlobalMenuWorkflow(lastBotCommandCode);
                    }

                    return lastBotCommandCode switch
                    {
                        (int)OperationsBotCommands.NewIssue => Option<WorkflowBase>.Some(NewIssueWorkflow),
                        _ => Option<WorkflowBase>.None()
                    };
                },
                Option<WorkflowBase>.None);
        }

        Option<TlgInput> GetBotCommandOfLastActiveWorkflow()
        {
            var botCommand = inputHistory.GetLastBotCommand();

            if (botCommand.IsNone)
                return Option<TlgInput>.None();
            
            // ToDo: replace with `i == lastBotCommand.GetValueOrThrow()` once I overloaded equals and == for TlgInput 
            var lastWorkflowHistory =
                inputHistory.GetLatestRecordsUpTo(i => 
                    i.TlgMessageId == botCommand.GetValueOrThrow().TlgMessageId &&
                    i.TlgDate == botCommand.GetValueOrThrow().TlgDate);
            
            var isWorkflowActive =
                !lastWorkflowHistory
                    .Any(i =>
                        i.ResultantWorkflow.IsSome &&
                        Glossary.GetDtType(i.ResultantWorkflow.GetValueOrThrow().InStateId)
                            .IsAssignableTo(typeof(IWorkflowStateTerminator)));

            return isWorkflowActive switch
            {
                true => botCommand,
                _ => Option<TlgInput>.None()
            };
        }
        
        Option<WorkflowBase> GetGlobalMenuWorkflow(int lastBotCommandCode)
        {
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
        
        Option<WorkflowBase> IdentifyCommunicationsWorkflow()
        {
            throw new NotImplementedException();
        }

        async Task<Option<WorkflowBase>> IdentifyNotificationsWorkflowAsync()
        {
            var currentInput = inputHistory.Last();

            var workflowBridge = 
                await BridgesRepo.GetAsync(currentInput.TlgAgent.ChatId, currentInput.TlgMessageId); 
            
            if (currentInput.InputType == TlgInputType.CallbackQuery && workflowBridge != null)
            {
                var currentControl = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

                // In the future I may have to look up what the SourceInput in the WorkflowBridge was, in order 
                // to determine which Workflow to identify here. E.g. I might ask: Do you want to accept this task?
                // With Yes/No ControlPrompts, and just clicking 'yes' on its own would not give enough context 
                // for the below identification. I need to understand what the 'yes' was an answer to.  
                
                return currentControl switch
                {
                    (long)ControlPrompts.ViewAttachments => 
                        Option<WorkflowBase>.Some(ViewAttachmentsWorkflow),
                    _ => 
                        throw new InvalidOperationException(
                            $"Unhandled {nameof(currentControl)}: '{currentControl}'.")
                };
            }
            
            return GetBotCommandOfLastActiveWorkflow().Match(
                cmd => 
                    GetGlobalMenuWorkflow(cmd.Details.BotCommandEnumCode.GetValueOrThrow()),
                Option<WorkflowBase>.None);
        }
    }

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}