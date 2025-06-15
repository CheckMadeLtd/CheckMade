using System.Collections.Immutable;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;
using static CheckMade.Common.Domain.Data.ChatBot.UserInteraction.InteractionMode;
using static CheckMade.Common.Domain.Data.ChatBot.Input.TlgInputType;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal interface IGeneralWorkflowUtils
{
    const int RecentLocationHistoryTimeFrameInMinutes = 2;
    
    static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("The previous workflow was completed. You can continue with a new one... "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    Task<IReadOnlyCollection<TlgInput>> GetAllCurrentInteractiveAsync(
        TlgAgent tlgAgentForDbQuery, TlgInput newInputToAppend);

    Task<IReadOnlyCollection<TlgInput>> GetInteractiveWorkflowHistoryAsync(TlgInput currentInput);
    Task<IReadOnlyCollection<TlgInput>> GetRecentLocationHistory(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<WorkflowBridge>> GetWorkflowBridgesOrNoneAsync(Option<ILiveEventInfo> liveEventInfo);
}

internal sealed record GeneralWorkflowUtils(
    ITlgInputsRepository InputsRepo,
    ITlgAgentRoleBindingsRepository TlgAgentRoleBindingsRepo,
    IDerivedWorkflowBridgesRepository BridgesRepo)
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
            .MaxBy(static tarb => tarb.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastExpiredRoleBind != null
            ? lastExpiredRoleBind.DeactivationDate.GetValueOrThrow()
            : DateTimeOffset.MinValue;

        var allInteractiveFromDb =
            await InputsRepo.GetAllInteractiveAsync(tlgAgentForDbQuery);

        var allInteractiveIncludingNewInput =
            allInteractiveFromDb.Concat([newInputToAppend]);
        
        var allCurrentInteractive = 
            allInteractiveIncludingNewInput
                .Where(i => i.TlgDate > cutOffDate)
                .ToList();

        return 
            allCurrentInteractive
                .ToImmutableArray();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInteractiveWorkflowHistoryAsync(
        TlgInput currentInput)
    {
        // Careful: if/when I decide to cache this, invalidate the cache after inputs are updated with new Guids!
        
        var currentRoleInputs = 
            await GetAllCurrentInteractiveAsync(
                currentInput.TlgAgent,
                currentInput);

        var allBridges = 
            await GetWorkflowBridgesOrNoneAsync(currentInput.LiveEventContext);
        
        return currentRoleInputs
            .GetLatestRecordsUpTo(input => input.IsWorkflowLauncher(allBridges))
            .ToImmutableArray();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetRecentLocationHistory(TlgAgent tlgAgent)
    {
        return 
            await InputsRepo.GetAllLocationAsync(
                tlgAgent, 
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
        this DomainTerm domainTerm, IReadOnlyCollection<TlgInput> inputHistory) =>
        inputHistory
            .Count(i => i.Details.DomainTerm.GetValueOrDefault() == domainTerm) 
        % 2 != 0;
    
    public static bool IsCurrentRoleTradeSpecific(this IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;

    public static bool IsWorkflowLauncher(
        this TlgInput input, IReadOnlyCollection<WorkflowBridge> allBridges)
    {
        var isProactiveWorkflowLauncher = 
            input.InputType == CommandMessage;
            
        var isReactiveWorkflowLauncher = 
            input is { InputType: CallbackQuery, TlgAgent.Mode: Notifications or Communications } &&
            allBridges.Any(b =>
                b.DestinationChatId == input.TlgAgent.ChatId &&
                b.DestinationMessageId == input.TlgMessageId);

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
