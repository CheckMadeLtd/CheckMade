using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
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
        var inputPort = new TlgClientPort(input.UserId, input.ChatId, input.InteractionMode);

        if (!await IsUserAuthenticated(inputPort, input.InteractionMode, portRoleRepo))
        {
            return await UserAuthWorkflow.CreateAsync(inputRepo, roleRepo, portRoleRepo);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, InteractionMode mode, ITlgClientPortRoleRepository portRoleRepo)
    {
        IReadOnlyList<TlgClientPortRole> tlgClientPortRoles =
            (await portRoleRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortRoles
                   .FirstOrDefault(cpmr => 
                       cpmr.ClientPort.ChatId == inputPort.ChatId &&
                       cpmr.ClientPort.Mode == mode &&
                       cpmr.Status == DbRecordStatus.Active) 
               != null;
    }
}