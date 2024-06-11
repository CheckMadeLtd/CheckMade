using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Utils;
using CheckMade.Telegram.Logic.Workflows;

namespace CheckMade.Telegram.Logic;

public interface IWorkflowIdentifier
{
    Task<Option<IWorkflow>> IdentifyAsync(TlgInput input);
}

internal class WorkflowIdentifier(
        ITlgInputRepository inputRepo,
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IWorkflowIdentifier
{
    public async Task<Option<IWorkflow>> IdentifyAsync(TlgInput input)
    {
        var inputPort = new TlgClientPort(input.UserId, input.ChatId);

        if (!await IsUserAuthenticated(inputPort, portToRoleMapRepo))
        {
            return new UserAuthWorkflow(inputRepo, portToRoleMapRepo);
        }
        
        return Option<IWorkflow>.None();
    }
    
    private static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, ITlgClientPortToRoleMapRepository mapRepo)
    {
        IReadOnlyList<TlgClientPortToRoleMap> tlgClientPortToRoleMap =
            (await mapRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortToRoleMap
                   .FirstOrDefault(map => map.ClientPort.ChatId == inputPort.ChatId &&
                                          map.Status == DbRecordStatus.Active) 
               != null;
    }
}