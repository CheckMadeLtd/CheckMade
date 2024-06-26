using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic;

internal interface ILogicUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);

    Task<IReadOnlyCollection<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent);
    
    public static Option<TlgInput> GetLastBotCommand(IReadOnlyCollection<TlgInput> inputs) =>
        inputs.LastOrDefault(i => i.Details.BotCommandEnumCode.IsSome)
        ?? Option<TlgInput>.None();
}

internal class LogicUtils(
        ITlgInputsRepository inputsRepo,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo)
    : ILogicUtils
{
    public async Task<IReadOnlyCollection<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent)
    {
        // We take this roundabout way to include inputs from currently unauthenticated users
        
        var lastPreviousTlgAgentRole = (await tlgAgentRoleBindingsRepo.GetAllAsync())
            .Where(arb =>
                arb.TlgAgent == tlgAgent &&
                arb.DeactivationDate.IsSome)
            .MaxBy(arb => arb.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastPreviousTlgAgentRole != null
            ? lastPreviousTlgAgentRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        return (await inputsRepo.GetAllAsync(tlgAgent))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent)
    {
        var allInputsOfTlgAgent = 
            await GetAllInputsOfTlgAgentInCurrentRoleAsync(tlgAgent);

        return allInputsOfTlgAgent
            .GetLatestRecordsUpTo(input => input.InputType == TlgInputType.CommandMessage)
            .ToImmutableReadOnlyCollection();
    }
}