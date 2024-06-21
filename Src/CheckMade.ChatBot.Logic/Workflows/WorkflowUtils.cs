using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("Previous workflow was completed. You can continue with a new one: "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    IReadOnlyCollection<TlgAgentRoleBind> GetAllTlgAgentRoles();
    Task<IReadOnlyCollection<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent);
}

internal class WorkflowUtils : IWorkflowUtils
{
    private readonly ITlgInputRepository _inputRepo;
    private readonly ITlgAgentRoleBindingsRepository _tlgAgentRoleBindingsRepo;
    
    private IReadOnlyCollection<TlgAgentRoleBind> _preExistingTlgAgentRoles = new List<TlgAgentRoleBind>();

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
        
        _preExistingTlgAgentRoles = getTlgAgentRolesTask.Result.ToImmutableReadOnlyCollection();
    }

    public IReadOnlyCollection<TlgAgentRoleBind> GetAllTlgAgentRoles() => _preExistingTlgAgentRoles;

    public async Task<IReadOnlyCollection<TlgInput>> GetAllInputsOfTlgAgentInCurrentRoleAsync(TlgAgent tlgAgent)
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
            .ToImmutableReadOnlyCollection();
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetInputsForCurrentWorkflow(TlgAgent tlgAgent)
    {
        var allInputsOfTlgAgent = 
            (await _inputRepo.GetAllAsync(tlgAgent)).ToImmutableReadOnlyCollection();

        return allInputsOfTlgAgent
            .GetLatestRecordsUpTo(input => input.InputType == TlgInputType.CommandMessage)
            .ToImmutableReadOnlyCollection();
    }
}