using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("Previous workflow was completed. You can continue with a new one: "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    IReadOnlyList<TlgAgentRoleBind> GetAllTlgAgentRoles();
    Task<IReadOnlyList<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent);
    Task<IReadOnlyList<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent);
}

internal class WorkflowUtils : IWorkflowUtils
{
    private readonly ITlgInputRepository _inputRepo;
    private readonly ITlgAgentRoleBindingsRepository _tlgAgentRoleBindingsRepo;
    
    private IReadOnlyList<TlgAgentRoleBind> _preExistingTlgAgentRoles = new List<TlgAgentRoleBind>();

    private WorkflowUtils(
        ITlgInputRepository inputRepo,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo)
    {
        _inputRepo = inputRepo;
        _tlgAgentRoleBindingsRepo = tlgAgentRoleBindingsRepo;
    }

    public static async Task<WorkflowUtils> CreateAsync(
        ITlgInputRepository inputRepo,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo)
    {
        var workflowUtils = new WorkflowUtils(inputRepo, tlgAgentRoleBindingsRepo);
        await workflowUtils.InitAsync();
        return workflowUtils;
    }
    
    private async Task InitAsync()
    {
        var getTlgAgentRolesTask = _tlgAgentRoleBindingsRepo.GetAllAsync();

        // In preparation for other async tasks that can then run in parallel
#pragma warning disable CA1842
        await Task.WhenAll(getTlgAgentRolesTask);
#pragma warning restore CA1842
        
        _preExistingTlgAgentRoles = getTlgAgentRolesTask.Result.ToList().AsReadOnly();
    }

    public IReadOnlyList<TlgAgentRoleBind> GetAllTlgAgentRoles() => _preExistingTlgAgentRoles;

    public async Task<IReadOnlyList<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent)
    {
        var lastPreviousTlgAgentRole = _preExistingTlgAgentRoles
            .Where(arb =>
                arb.TlgAgent == tlgAgent &&
                arb.DeactivationDate.IsSome)
            .MaxBy(arb => arb.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastPreviousTlgAgentRole != null
            ? lastPreviousTlgAgentRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        return (await _inputRepo.GetAllAsync(tlgAgent))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent)
    {
        var allInputsOfTlgAgent = 
            (await _inputRepo.GetAllAsync(tlgAgent)).ToList().AsReadOnly();

        return GetLatestRecordsUpTo(
            allInputsOfTlgAgent,
            input => input.InputType == TlgInputType.CommandMessage)
            .ToList().AsReadOnly();
    }

    private static IEnumerable<T> GetLatestRecordsUpTo<T>(
        IReadOnlyCollection<T> collection, Func<T, bool> stopCondition, bool includeStopItem = true)
    {
        var result = collection.Reverse()
            .TakeWhile(item => !stopCondition(item))
            .ToList();

        if (includeStopItem)
        {
            var firstItemMeetingCondition = collection.LastOrDefault(stopCondition);

            if (firstItemMeetingCondition != null)
                result.Add(firstItemMeetingCondition);
        }

        result.Reverse();
        
        return result.ToImmutableList().AsReadOnly();
    }
}