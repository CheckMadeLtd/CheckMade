using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
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
        ITlgClientPortModeRoleRepository portModeRoleRepo) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(TlgInput input)
    {
        var inputPort = new TlgClientPort(input.UserId, input.ChatId);

        if (!await IsUserAuthenticated(inputPort, portModeRoleRepo))
        {
            return await UserAuthWorkflow.CreateAsync(inputRepo, roleRepo, portModeRoleRepo);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, ITlgClientPortModeRoleRepository portModeRoleRepo)
    {
        IReadOnlyList<TlgClientPortModeRole> tlgClientPortModeRoles =
            (await portModeRoleRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortModeRoles
                   .FirstOrDefault(cpr => cpr.ClientPort.ChatId == inputPort.ChatId &&
                                          cpr.Status == DbRecordStatus.Active) 
               != null;
    }
}