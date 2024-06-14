using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

public interface IWorkflowIdentifier
{
    Task<Option<IWorkflow>> IdentifyAsync(TlgInput input);
}

internal class WorkflowIdentifier(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(TlgInput input)
    {
        var inputPort = new TlgClientPort(input.UserId, input.ChatId);

        if (!await IsUserAuthenticated(inputPort, portRoleRepo))
        {
            return await UserAuthWorkflow.CreateAsync(inputRepo, roleRepo, portRoleRepo);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, ITlgClientPortRoleRepository portRoleRepo)
    {
        IReadOnlyList<TlgClientPortRole> tlgClientPortRoles =
            (await portRoleRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortRoles
                   .FirstOrDefault(cpr => cpr.ClientPort.ChatId == inputPort.ChatId &&
                                          cpr.Status == DbRecordStatus.Active) 
               != null;
    }
}