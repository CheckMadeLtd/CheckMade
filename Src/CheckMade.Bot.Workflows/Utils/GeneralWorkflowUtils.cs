using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Common.Actors;
using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.LiveEvents;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using static CheckMade.Abstract.Domain.Model.Bot.Categories.InteractionMode;
using static CheckMade.Abstract.Domain.Model.Bot.Categories.InputType;

namespace CheckMade.Bot.Workflows.Utils;

public interface IGeneralWorkflowUtils
{
    const int RecentLocationHistoryTimeFrameInMinutes = 2;
    
    static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    Task<IReadOnlyCollection<Input>> GetAllCurrentInteractiveAsync(
        Agent agentForDbQuery, Input newInputToAppend);

    Task<IReadOnlyCollection<Input>> GetInteractiveWorkflowHistoryAsync(Input currentInput);
    Task<IReadOnlyCollection<Input>> GetRecentLocationHistory(Agent agent);
    Task<IReadOnlyCollection<WorkflowBridge>> GetWorkflowBridgesOrNoneAsync(Option<ILiveEventInfo> liveEventInfo);
}

public sealed record GeneralWorkflowUtils(
    IInputsRepository InputsRepo,
    IAgentRoleBindingsRepository AgentRoleBindingsRepo,
    IDerivedWorkflowBridgesRepository BridgesRepo)
    : IGeneralWorkflowUtils
{
    public async Task<IReadOnlyCollection<Input>> GetAllCurrentInteractiveAsync(
        Agent agentForDbQuery,
        Input newInputToAppend)
    {
        // This is designed to ensure that inputs from new, currently unauthenticated users are included
        // Careful: if/when I decide to cache this, invalidate the cache after inputs are updated with new Guids!
        
        var lastExpiredRoleBind = (await AgentRoleBindingsRepo.GetAllAsync())
            .Where(arb =>
                arb.Agent.Equals(agentForDbQuery) &&
                arb.DeactivationDate.IsSome)
            .MaxBy(static arb => arb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTimeOffset.MinValue;

        var allInteractiveFromDb =
            await InputsRepo.GetAllInteractiveAsync(agentForDbQuery);

        var allInteractiveIncludingNewInput =
            allInteractiveFromDb.Concat([newInputToAppend]);
        
        var allCurrentInteractive = 
            allInteractiveIncludingNewInput
                .Where(i => i.TimeStamp > cutOffDate)
                .ToList();

        return 
            allCurrentInteractive
                .ToImmutableArray();
    }

    public async Task<IReadOnlyCollection<Input>> GetInteractiveWorkflowHistoryAsync(
        Input currentInput)
    {
        // Careful: if/when I decide to cache this, invalidate the cache after inputs are updated with new Guids!
        
        var currentRoleInputs = 
            await GetAllCurrentInteractiveAsync(
                currentInput.Agent,
                currentInput);

        var allBridges = 
            await GetWorkflowBridgesOrNoneAsync(currentInput.LiveEventContext);
        
        return currentRoleInputs
            .GetLatestRecordsUpTo(input => input.IsWorkflowLauncher(allBridges))
            .ToImmutableArray();
    }

    public async Task<IReadOnlyCollection<Input>> GetRecentLocationHistory(Agent agent)
    {
        return 
            await InputsRepo.GetAllLocationAsync(
                agent, 
                DateTimeOffset.UtcNow
                    .AddMinutes(-IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes));
    }

    public async Task<IReadOnlyCollection<WorkflowBridge>> GetWorkflowBridgesOrNoneAsync(
        Option<ILiveEventInfo> liveEventInfo) =>
        liveEventInfo.IsSome
            ? await BridgesRepo.GetAllAsync(liveEventInfo.GetValueOrThrow())
            : [];
}

internal static class GeneralWorkflowUtilsExtensions
{
    public static string GetTypeNameWithoutGenericParamSuffix(this string typeName) =>
        typeName.Split('`')[0];
    
    public static bool IsToggleOn(
        this DomainTerm domainTerm, IReadOnlyCollection<Input> inputHistory) =>
        inputHistory
            .Count(i => i.Details.DomainTerm.GetValueOrDefault() == domainTerm) 
        % 2 != 0;
    
    public static bool IsCurrentRoleTradeSpecific(this IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;

    public static bool IsWorkflowLauncher(
        this Input input, IReadOnlyCollection<WorkflowBridge> allBridges)
    {
        var isProactiveWorkflowLauncher = 
            input.InputType == CommandMessage;
            
        var isReactiveWorkflowLauncher = 
            input is { InputType: CallbackQuery, Agent.Mode: Notifications or Communications } &&
            allBridges.Any(b =>
                b.DestinationChatId == input.Agent.ChatId &&
                b.DestinationMessageId == input.MessageId);

        return isProactiveWorkflowLauncher || isReactiveWorkflowLauncher;
    }
    
    public static IReadOnlyCollection<T> GetLatestRecordsUpTo<T>(
        this IEnumerable<T> enumerable, Func<T, bool> stopCondition, bool includeStopItem = true)
    {
        var enumeratedDesc = enumerable.Reverse().ToList(); // .Reverse() required for usage of .TakeWhile()
        
        if (enumeratedDesc.Count == 0)
            return ImmutableList<T>.Empty;
        
        var result = enumeratedDesc
            .TakeWhile(item => !stopCondition(item))
            .ToList();

        if (includeStopItem)
        {
            var firstItemMeetingCondition = enumeratedDesc.FirstOrDefault(stopCondition);

            if (firstItemMeetingCondition != null)
                result.Add(firstItemMeetingCondition);
        }

        result.Reverse(); // back to the original ASC order
        
        return result.ToImmutableList();
    }
}
