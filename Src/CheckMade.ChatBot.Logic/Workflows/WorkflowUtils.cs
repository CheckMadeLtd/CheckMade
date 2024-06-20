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
    Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent);
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

    public async Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent)
    {
        var lastUsedTlgAgentRole = _preExistingTlgAgentRoles
            .Where(cpr =>
                cpr.TlgAgent == tlgAgent &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgAgentRole != null
            ? lastUsedTlgAgentRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        // ToDo: modify GetAllAsync so that it also queries for mode i.e. the entire TlgAgent !!!
        // Otherwise interference in workflow recognition across InteractionModes as already happened. 
        return (await _inputRepo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToList().AsReadOnly();
    }
}