using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowUtils
{
    public static readonly UiString WorkflowWasCompleted = UiConcatenate(
        Ui("Previous workflow was completed. You can continue with a new one: "),
        IInputProcessor.SeeValidBotCommandsInstruction);
    
    IReadOnlyList<TlgClientPortRole> GetAllClientPortRoles();
    Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent);
}

internal class WorkflowUtils : IWorkflowUtils
{
    private readonly ITlgInputRepository _inputRepo;
    private readonly ITlgClientPortRoleRepository _portRoleRepo;
    
    private IReadOnlyList<TlgClientPortRole> _preExistingPortRoles = new List<TlgClientPortRole>();

    private WorkflowUtils(
        ITlgInputRepository inputRepo,
        ITlgClientPortRoleRepository portRoleRepo)
    {
        _inputRepo = inputRepo;
        _portRoleRepo = portRoleRepo;
    }

    public static async Task<WorkflowUtils> CreateAsync(
        ITlgInputRepository inputRepo,
        ITlgClientPortRoleRepository portRoleRepo)
    {
        var workflowUtils = new WorkflowUtils(inputRepo, portRoleRepo);
        await workflowUtils.InitAsync();
        return workflowUtils;
    }
    
    private async Task InitAsync()
    {
        var getPortRolesTask = _portRoleRepo.GetAllAsync();

        // In preparation for other async tasks that can then run in parallel
#pragma warning disable CA1842
        await Task.WhenAll(getPortRolesTask);
#pragma warning restore CA1842
        
        _preExistingPortRoles = getPortRolesTask.Result.ToList().AsReadOnly();
    }

    public IReadOnlyList<TlgClientPortRole> GetAllClientPortRoles() => _preExistingPortRoles;

    public async Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgAgent tlgAgent)
    {
        var lastUsedTlgClientPortRole = _preExistingPortRoles
            .Where(cpr =>
                cpr.TlgAgent == tlgAgent &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
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