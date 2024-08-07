using System.ComponentModel;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

internal interface IWorkflowIdentifier
{
    Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory);
}

internal sealed record WorkflowIdentifier(
        IUserAuthWorkflow UserAuthWorkflow,
        INewIssueWorkflow NewIssueWorkflow,
        ILanguageSettingWorkflow LanguageSettingWorkflow,
        ILogoutWorkflow LogoutWorkflow,
        IViewAttachmentsWorkflow ViewAttachmentsWorkflow,
        IDomainGlossary Glossary) 
    : IWorkflowIdentifier
{
    public Option<IWorkflow> Identify(IReadOnlyCollection<TlgInput> inputHistory)
    {
        if (!IsUserAuthenticated(inputHistory))
            return Option<IWorkflow>.Some(UserAuthWorkflow);

        var currentMode = inputHistory.Last().TlgAgent.Mode;

        return currentMode switch
        {
            InteractionMode.Operations => IdentifyOperationsWorkflow(),
            InteractionMode.Communications => IdentifyCommunicationsWorkflow(),
            InteractionMode.Notifications => IdentifyNotificationsWorkflow(),
            _ => throw new InvalidEnumArgumentException($"Unhandled {nameof(currentMode)}")
        };
        
        Option<IWorkflow> IdentifyOperationsWorkflow()
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
                        (int)OperationsBotCommands.NewIssue => Option<IWorkflow>.Some(NewIssueWorkflow),
                        _ => Option<IWorkflow>.None()
                    };
                },
                Option<IWorkflow>.None);
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
        
        Option<IWorkflow> GetGlobalMenuWorkflow(int lastBotCommandCode)
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
                        $"'{nameof(BotCommandMenus.GlobalBotCommandsCodeThreshold_90)}'")
            };
        }
        
        Option<IWorkflow> IdentifyCommunicationsWorkflow()
        {
            throw new NotImplementedException();
        }

        Option<IWorkflow> IdentifyNotificationsWorkflow()
        {
            var currentInput = inputHistory.Last();

            if (currentInput.InputType == TlgInputType.CallbackQuery)
            {
                var currentControl = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

                return currentControl switch
                {
                    (long)ControlPrompts.ViewAttachments => 
                        Option<IWorkflow>.Some(ViewAttachmentsWorkflow),
                    _ => 
                        throw new InvalidOperationException(
                            $"Unhandled {nameof(currentControl)}: '{currentControl}'.")
                };
            }
            
            return GetBotCommandOfLastActiveWorkflow().Match(
                cmd => 
                    GetGlobalMenuWorkflow(cmd.Details.BotCommandEnumCode.GetValueOrThrow()),
                Option<IWorkflow>.None);
        }
    }

    private static bool IsUserAuthenticated(IReadOnlyCollection<TlgInput> inputHistory) => 
        inputHistory.Count != 0 && 
        inputHistory.Last().OriginatorRole.IsSome;
}