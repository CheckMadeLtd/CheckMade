using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal interface IGeneralWorkflowUtils
{
    const int RecentLocationHistoryTimeFrameInMinutes = 2;
    
    static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgentForDbQuery, TlgInput newInputToAppend);

    Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommandAsync(TlgInput currentInput);
    Task<IReadOnlyCollection<TlgInput>> GetRecentLocationHistory(TlgAgent tlgAgent);
    Task<Type> GetPreviousResultantStateTypeAsync(TlgInput currentInput);
    bool IsWorkflowTerminated(IReadOnlyCollection<TlgInput> inputHistory);
}

internal sealed record GeneralWorkflowUtils(
    ITlgInputsRepository InputsRepo,
    ITlgAgentRoleBindingsRepository TlgAgentRoleBindingsRepo,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary)
    : IGeneralWorkflowUtils
{
    public async Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgentForDbQuery,
        TlgInput newInputToAppend)
    {
        // This is designed to ensure that inputs from new, currently unauthenticated users are included
        // Careful: if/when I decide to cache this, invalidate the cache after inputs are updated with new Guids!
        
        var lastExpiredRoleBind = (await TlgAgentRoleBindingsRepo.GetAllAsync())
            .Where(tarb =>
                tarb.TlgAgent.Equals(tlgAgentForDbQuery) &&
                tarb.DeactivationDate.IsSome)
            .MaxBy(tarb => tarb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTimeOffset.MinValue;

        var allInteractiveFromDb =
            await InputsRepo.GetAllInteractiveAsync(tlgAgentForDbQuery);

        var allInteractiveIncludingNewInput =
            allInteractiveFromDb.Concat(new[] { newInputToAppend });
        
        var allCurrentInteractive = 
            allInteractiveIncludingNewInput
                .Where(i =>
                    i.TlgDate.ToUniversalTime() >
                    cutOffDate.ToUniversalTime())
                .ToList();

        return 
            allCurrentInteractive
                .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInteractiveSinceLastBotCommandAsync(
        TlgInput currentInput)
    {
        // Careful: if/when I decide to cache this, invalidate the cache after inputs are updated with new Guids!
        
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
                DateTimeOffset.UtcNow
                    .AddMinutes(-IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes));
    }

    public async Task<Type> GetPreviousResultantStateTypeAsync(TlgInput currentInput)
    {
        var interactiveHistory =
            await GetInteractiveSinceLastBotCommandAsync(currentInput);

        if (interactiveHistory.Count <= 1)
            throw new InvalidOperationException("Interactive History is too short for this function");

        var lastInput =
            interactiveHistory
                .SkipLast(1)
                .Last();

        if (lastInput.ResultantWorkflow.IsNone)
            throw new InvalidOperationException($"The last input has no {nameof(lastInput.ResultantWorkflow)}");

        return
            Glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId);
    }

    public bool IsWorkflowTerminated(IReadOnlyCollection<TlgInput> inputHistory)
    {
        return
            inputHistory.Any(i =>
                i.ResultantWorkflow.IsSome &&
                Glossary.GetDtType(
                        i.ResultantWorkflow.GetValueOrThrow().InStateId)
                    .IsAssignableTo(typeof(IWorkflowStateTerminator)));
    }

    public async Task<bool> IsWorkflowLauncherAsync(TlgInput input)
    {
        if (input.InputType == TlgInputType.CommandMessage)
            return true;

        return input.InputType == TlgInputType.CallbackQuery && 
               await IsDestinationOfWorkflowBridgeAsync();

        async Task<bool> IsDestinationOfWorkflowBridgeAsync()
        {
            if (input.LiveEventContext.IsNone)
                return false;
            
            var allBridges = 
                await BridgesRepo.GetAllAsync(input.LiveEventContext.GetValueOrThrow());

            return allBridges.Any(b =>
                b.DestinationChatId == input.TlgAgent.ChatId &&
                b.DestinationMessageId == input.TlgMessageId);
        }
    }
}

internal static class GeneralWorkflowUtilsExtensions
{
    public static string GetTypeNameWithoutGenericParamSuffix(this string typeName) =>
        typeName.Split('`')[0];
    
    public static bool IsToggleOn(this DomainTerm domainTerm, IReadOnlyCollection<TlgInput> inputHistory) =>
        inputHistory
            .Count(i => i.Details.DomainTerm.GetValueOrDefault() == domainTerm) 
        % 2 != 0;
    
    public static Option<TlgInput> GetLastBotCommand(this IReadOnlyCollection<TlgInput> inputs) =>
        inputs.LastOrDefault(i => 
            i.Details.BotCommandEnumCode.IsSome) 
        ?? Option<TlgInput>.None();
    
    public static bool IsCurrentRoleTradeSpecific(this IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;
}
