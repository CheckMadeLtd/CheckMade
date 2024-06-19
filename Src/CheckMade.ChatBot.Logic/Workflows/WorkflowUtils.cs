using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowUtils
{
    Task<IReadOnlyList<TlgClientPortRole>> GetAllClientPortRolesAsync();
    Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgClientPort clientPort);
}

internal class WorkflowUtils : IWorkflowUtils
{
    private readonly ITlgInputRepository _inputRepo;
    private readonly ITlgClientPortRoleRepository _portRoleRepo;
    
    private IEnumerable<TlgClientPortRole> _preExistingPortRoles = new List<TlgClientPortRole>();

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

    public async Task<IReadOnlyList<TlgClientPortRole>> GetAllClientPortRolesAsync() =>
        (await _portRoleRepo.GetAllAsync())
        .ToList().AsReadOnly();

    public async Task<IReadOnlyList<TlgInput>> GetAllCurrentInputsAsync(TlgClientPort clientPort)
    {
        var lastUsedTlgClientPortRole = _preExistingPortRoles
            .Where(cpr =>
                cpr.ClientPort == clientPort &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        return (await _inputRepo.GetAllAsync(clientPort.UserId, clientPort.ChatId))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToList().AsReadOnly();
    }
}