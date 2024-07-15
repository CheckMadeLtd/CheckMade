using CheckMade.Common.Interfaces.ChatBot.Logic;
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
        ILogicUtils logicUtils,
        IDomainGlossary glossary) 
    : ILogoutWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        return DetermineCurrentState(inputHistory, inputHistory.LastOrDefault()) == LogoutConfirmed;
    }

    public async Task<Result<WorkflowResponse>> 
        GetResponseAsync(TlgInput currentInput)
    {
        var workflowInputHistory = 
            await logicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var currentRoleBind = (await roleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.TlgAgent.Equals(currentInput.TlgAgent));

        return DetermineCurrentState(workflowInputHistory, currentInput) switch
        {
            Initial => 
                new WorkflowResponse(
                    new List<OutputDto> { new() 
                        { 
                            Text = UiConcatenate(
                                Ui("{0}, your current role is: ", 
                                    currentRoleBind.Role.ByUser.FirstName),
                                glossary.GetUi(currentRoleBind.Role.RoleType.GetType()),
                                UiNoTranslate(".\n"),
                                Ui("""
                                    Are you sure you want to log out from this chat for {0}?
                                    FYI: You will also be logged out from other non-group bot chats in this role.
                                    """, 
                                currentRoleBind.Role.AtLiveEvent.Name)),
                            
                            ControlPromptsSelection = ControlPrompts.YesNo 
                        } 
                    }, 
                    glossary.GetId(Initial)),
            
            LogoutConfirmed => 
                new WorkflowResponse(
                    await PerformLogoutAsync(currentRoleBind),
                    glossary.GetId(LogoutConfirmed)),
            
            LogoutAborted => 
                new WorkflowResponse(
                    new List<OutputDto> { new() 
                        { 
                            Text = UiConcatenate(
                            Ui("Logout aborted.\n"),
                            IInputProcessor.SeeValidBotCommandsInstruction) 
                        } 
                    },
                    glossary.GetId(LogoutAborted)),
            
            _ => Result<WorkflowResponse>.FromError(
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