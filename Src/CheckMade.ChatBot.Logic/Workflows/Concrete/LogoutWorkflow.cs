using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILogoutWorkflow : IWorkflow
{
    LogoutWorkflow.States DetermineCurrentState(IReadOnlyCollection<TlgInput> history);
}

internal class LogoutWorkflow(
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils) 
    : ILogoutWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> history)
    {
        return DetermineCurrentState(history) == States.LogoutConfirmed;
    }

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var recentHistory = await logicUtils.GetInputsForCurrentWorkflow(tlgInput.TlgAgent);

        var currentRoleBind = (await roleBindingsRepo.GetAllAsync())
            .First(arb => 
                arb.TlgAgent == tlgInput.TlgAgent &&
                arb.Status == DbRecordStatus.Active);

        return DetermineCurrentState(recentHistory) switch
        {
            States.Initial => new List<OutputDto>
            {
                new()
                {
                    Text = Ui("""
                              {0}, are you sure you want to log out from this chat in your role as {1} for {2}?
                              FYI: You will also be logged out from other non-group bot chats in this role.
                              """,
                        currentRoleBind.Role.User.FirstName,
                        currentRoleBind.Role.RoleType,
                        currentRoleBind.Role.LiveEvent.Name),
                    
                    ControlPromptsSelection = ControlPrompts.YesNo
                }
            },
            
            States.LogoutConfirmed => await PerformLogoutAsync(currentRoleBind),
            
            States.LogoutAborted => new List<OutputDto>
            {
                new()
                {
                    Text = UiConcatenate(
                        Ui("Logout aborted.\n"),
                        IInputProcessor.SeeValidBotCommandsInstruction)
                }
            },
            
            _ => Result<IReadOnlyCollection<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(LogoutWorkflow)}"))
        };
    }

    public States DetermineCurrentState(IReadOnlyCollection<TlgInput> history)
    {
        var lastInput = history.Last();

        if (lastInput.InputType == TlgInputType.CallbackQuery)
        {
            return lastInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
            {
                (int)ControlPrompts.Yes => States.LogoutConfirmed,
                (int)ControlPrompts.No => States.LogoutAborted,
                _ => throw new ArgumentOutOfRangeException(nameof(lastInput), 
                    "Unexpected value for ControlPromptEnumCode")
            };
        }

        return States.Initial;
    }

    private async Task<List<OutputDto>> PerformLogoutAsync(TlgAgentRoleBind currentRoleBind)
    {
        var roleBindingsToUpdateIncludingOtherModesInCaseOfPrivateChat = 
            (await roleBindingsRepo.GetAllAsync())
            .Where(arb =>
                arb.TlgAgent.UserId == currentRoleBind.TlgAgent.UserId &&
                arb.TlgAgent.ChatId == currentRoleBind.TlgAgent.ChatId &&
                arb.Role.Token == currentRoleBind.Role.Token &&
                arb.Status == DbRecordStatus.Active)
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