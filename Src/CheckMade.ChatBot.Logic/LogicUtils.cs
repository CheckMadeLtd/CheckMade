using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic;

internal interface ILogicUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);

    Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetInputsSinceLastBotCommand(TlgAgent tlgAgent);
    
    public static Option<TlgInput> GetLastBotCommand(IReadOnlyCollection<TlgInput> inputs) =>
        inputs.LastOrDefault(i => 
            i.Details.BotCommandEnumCode.IsSome) 
        ?? Option<TlgInput>.None();
}

internal class LogicUtils(
        ITlgInputsRepository inputsRepo,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo)
    : ILogicUtils
{
    public async Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent)
    {
        // This is designed to ensure that inputs from new, currently unauthenticated users are included
        
        var lastExpiredRoleBind = (await tlgAgentRoleBindingsRepo.GetAllAsync())
            .Where(tarb =>
                tarb.TlgAgent.Equals(tlgAgent) &&
                tarb.DeactivationDate.IsSome)
            .MaxBy(tarb => tarb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        return (await inputsRepo.GetAllUserInitiatedAsync(tlgAgent))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                cutOffDate.ToUniversalTime())
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInputsSinceLastBotCommand(TlgAgent tlgAgent)
    {
        var currentRoleInputs = await GetAllCurrentInputsAsync(tlgAgent);

        return currentRoleInputs
            .GetLatestRecordsUpTo(input => input.InputType.Equals(TlgInputType.CommandMessage))
            .ToImmutableReadOnlyCollection();
    }
}