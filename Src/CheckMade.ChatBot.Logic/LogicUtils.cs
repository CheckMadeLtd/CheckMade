using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic;

internal interface ILogicUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgent, TlgInput? appendCurrentInputToHistory = null);
    
    Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommand(TlgInput currentInput);
    
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
    public async Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgent,
        // appendCurrentInputToHistory - used in case this method is called BEFORE currentInput was added to DB!
        TlgInput? appendCurrentInputToHistory = null)
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
        
        var allCurrentInteractiveFromDb = (await inputsRepo.GetAllInteractiveAsync(tlgAgent))
            .Where(i =>
                i.Details.TlgDate.ToUniversalTime() >
                cutOffDate.ToUniversalTime())
            .ToList();

        if (appendCurrentInputToHistory is not null)
            allCurrentInteractiveFromDb
                .Add(appendCurrentInputToHistory);

        return 
            allCurrentInteractiveFromDb
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommand(TlgInput currentInput)
    {
        var currentRoleInputs = 
            await GetAllCurrentInteractiveAsync(
                currentInput.TlgAgent,
                currentInput);
        
        return currentRoleInputs
            .GetLatestRecordsUpTo(input => input.InputType.Equals(TlgInputType.CommandMessage))
            .ToImmutableReadOnlyCollection();
    }
}