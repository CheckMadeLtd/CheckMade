using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

using static LogoutWorkflow.States;

internal interface ILogoutWorkflow : IWorkflow
{
    LogoutWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory,
        TlgInput? currentInput);
}

internal class LogoutWorkflow(
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils) 
    : ILogoutWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        return DetermineCurrentState(inputHistory, inputHistory.LastOrDefault()) == LogoutConfirmed;
    }

    public async Task<Result<(IReadOnlyCollection<OutputDto> Output, Option<Enum> NewState)>> 
        GetResponseAsync(TlgInput currentInput)
    {
        var workflowInputHistory = 
            await logicUtils.GetInteractiveSinceLastBotCommand(currentInput);

        var currentRoleBind = (await roleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(currentInput.TlgAgent));

        return DetermineCurrentState(workflowInputHistory, currentInput) switch
        {
            Initial => 
                (new List<OutputDto> { new() 
                    { 
                        Text = Ui("""
                                {0}, are you sure you want to log out from this chat in your role as {1} for {2}?
                                FYI: You will also be logged out from other non-group bot chats in this role.
                                """, 
                            currentRoleBind.Role.ByUser.FirstName, 
                            currentRoleBind.Role.RoleType, 
                            currentRoleBind.Role.AtLiveEvent.Name),
                        
                        ControlPromptsSelection = ControlPrompts.YesNo 
                    }
            }, Initial),
            
            LogoutConfirmed => 
                (await PerformLogoutAsync(currentRoleBind),
                    LogoutConfirmed),
            
            LogoutAborted => 
                (new List<OutputDto> { new() 
                    { 
                        Text = UiConcatenate(
                        Ui("Logout aborted.\n"),
                        IInputProcessor.SeeValidBotCommandsInstruction) 
                    }
                }, LogoutAborted),
            
            _ => Result<(IReadOnlyCollection<OutputDto>, Option<Enum>)>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LogoutWorkflow)}"))
        };
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory,
        TlgInput? currentInput)
    {
        if (currentInput is null)
            return Initial;
        
        if (currentInput.InputType.Equals(TlgInputType.CallbackQuery))
        {
            return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
            {
                (int)ControlPrompts.Yes => LogoutConfirmed,
                (int)ControlPrompts.No => LogoutAborted,
                _ => throw new ArgumentOutOfRangeException(nameof(currentInput), 
                    "Unexpected value for ControlPromptEnumCode")
            };
        }

        return Initial;
    }

    private async Task<List<OutputDto>> PerformLogoutAsync(TlgAgentRoleBind currentRoleBind)
    {
        var roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat = 
            (await roleBindingsRepo.GetAllActiveAsync())
            .Where(tarb =>
                tarb.TlgAgent.UserId.Equals(currentRoleBind.TlgAgent.UserId) &&
                tarb.TlgAgent.ChatId.Equals(currentRoleBind.TlgAgent.ChatId) &&
                tarb.Role.Token.Equals(currentRoleBind.Role.Token))
            .ToImmutableReadOnlyCollection();
        
        await roleBindingsRepo
            .UpdateStatusAsync(
                roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat, 
                DbRecordStatus.Historic);
        
        return [new OutputDto 
        {
            Text = Ui("💨 Logged out.")
        }];
    }

    [Flags]
    internal enum States
    {
        Initial = 1,
        LogoutConfirmed = 1<<1,
        LogoutAborted = 1<<2
    }
}