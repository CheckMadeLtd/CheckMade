using CheckMade.Common.Interfaces.ChatBot.Logic;
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
    Task<string> GetPreviousStateNameAsync(TlgInput currentInput, int indexFromCurrent);
}

internal record LogicUtils(
        ITlgInputsRepository InputsRepo,
        ITlgAgentRoleBindingsRepository TlgAgentRoleBindingsRepo,
        IDomainGlossary Glossary)
    : ILogicUtils
{
    public async Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgentForDbQuery,
        TlgInput newInputToAppend)
    {
        // This is designed to ensure that inputs from new, currently unauthenticated users are included
        
        var lastExpiredRoleBind = (await TlgAgentRoleBindingsRepo.GetAllAsync())
            .Where(tarb =>
                tarb.TlgAgent.Equals(tlgAgentForDbQuery) &&
                tarb.DeactivationDate.IsSome)
            .MaxBy(tarb => tarb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;

        var allInteractiveFromDb =
            await InputsRepo.GetAllInteractiveAsync(tlgAgentForDbQuery);

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

    public async Task<IReadOnlyCollection<TlgInput>> 
        GetInteractiveSinceLastBotCommandAsync(TlgInput currentInput)
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
            await InputsRepo.GetAllLocationAsync(
                tlgAgent, 
                DateTime.UtcNow
                    .AddMinutes(-ILogicUtils.RecentLocationHistoryTimeFrameInMinutes));
    }

    public async Task<string> GetPreviousStateNameAsync(TlgInput currentInput, int indexFromCurrent)
    {
        var interactiveHistory =
            await GetInteractiveSinceLastBotCommandAsync(currentInput);

        if (interactiveHistory.Count < -(indexFromCurrent - 1))
            throw new InvalidOperationException("Interactive History is too short for this function");

        var lastInput =
            interactiveHistory
                .SkipLast(-indexFromCurrent)
                .Last();

        if (lastInput.ResultantWorkflow.IsNone)
            throw new InvalidOperationException($"The last input has no {nameof(lastInput.ResultantWorkflow)}");
        
        return
            Glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId)
                .Name
                .GetTypeNameWithoutGenericParam();
    }
}

internal static class LogicUtilsExtensions
{
    public static string GetTypeNameWithoutGenericParam(this string typeName) =>
        typeName.Split('`')[0];
    
    public static bool IsToggleOn(this DomainTerm domainTerm, IReadOnlyCollection<TlgInput> inputHistory) =>
        inputHistory
            .Count(i => i.Details.DomainTerm.GetValueOrDefault() == domainTerm) 
        % 2 != 0;
    
    public static Option<TlgInput> GetLastBotCommand(this IReadOnlyCollection<TlgInput> inputs) =>
        inputs.LastOrDefault(i => 
            i.Details.BotCommandEnumCode.IsSome) 
        ?? Option<TlgInput>.None();
}
