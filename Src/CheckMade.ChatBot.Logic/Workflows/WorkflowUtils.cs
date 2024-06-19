using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowUtils
{
    Task<IReadOnlyList<TlgInput>> GetAllCurrentInputs(
        TlgUserId userId, TlgChatId chatId, InteractionMode mode);
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
    
    public async Task<IReadOnlyList<TlgInput>> GetAllCurrentInputs(
        TlgUserId userId, TlgChatId chatId, InteractionMode mode)
    {
        var lastUsedTlgClientPortRole = _preExistingPortRoles
            .Where(cpr =>
                cpr.ClientPort == new TlgClientPort(userId, chatId, mode) &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        return (await _inputRepo.GetAllAsync(userId, chatId))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToList().AsReadOnly();
    }
}