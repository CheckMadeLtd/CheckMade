using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic;

internal interface ILogicUtils
{
    const int RecentLocationHistoryTimeFrameInMinutes = 2;
    
    static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgentForDbQuery, TlgInput newInputToAppend);
    Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommandAsync(TlgInput currentInput);
    Task<IReadOnlyCollection<TlgInput>> GetRecentLocationHistory(TlgAgent tlgAgent);
    
    static Option<TlgInput> GetLastBotCommand(IReadOnlyCollection<TlgInput> inputs) =>
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
        TlgAgent tlgAgentForDbQuery,
        TlgInput newInputToAppend)
    {
        // This is designed to ensure that inputs from new, currently unauthenticated users are included
        
        var lastExpiredRoleBind = (await tlgAgentRoleBindingsRepo.GetAllAsync())
            .Where(tarb =>
                tarb.TlgAgent.Equals(tlgAgentForDbQuery) &&
                tarb.DeactivationDate.IsSome)
            .MaxBy(tarb => tarb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;

        var allInteractiveFromDb =
            await inputsRepo.GetAllInteractiveAsync(tlgAgentForDbQuery);

        var allInteractiveIncludingNewInput =
            allInteractiveFromDb.Concat(new[] { newInputToAppend });
        
        var allCurrentInteractive = 
            allInteractiveIncludingNewInput
            .Where(i =>
                i.Details.TlgDate.ToUniversalTime() >
                cutOffDate.ToUniversalTime())
            .ToList();

        return 
            allCurrentInteractive
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommandAsync(TlgInput currentInput)
    {
        var currentRoleInputs = 
            await GetAllCurrentInteractiveAsync(
                currentInput.TlgAgent,
                currentInput);
        
        return currentRoleInputs
            .GetLatestRecordsUpTo(input => input.InputType.Equals(TlgInputType.CommandMessage))
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetRecentLocationHistory(TlgAgent tlgAgent)
    {
        return 
            await inputsRepo.GetAllLocationAsync(
                tlgAgent, 
                DateTime.UtcNow
                    .AddMinutes(-ILogicUtils.RecentLocationHistoryTimeFrameInMinutes));
    }
}