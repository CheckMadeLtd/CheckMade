using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILogoutWorkflow : IWorkflow;

internal class LogoutWorkflow(ITlgAgentRoleBindingsRepository roleBindingsRepo) : ILogoutWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> history)
    {
        return true;
    }

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var currentRoleBind = (await roleBindingsRepo.GetAllAsync())
            .First(arb => arb.TlgAgent == tlgInput.TlgAgent);

        // ToDo: also delete related roleBindings for same ChatId for the other modes!
        await roleBindingsRepo.UpdateStatusAsync(currentRoleBind, DbRecordStatus.Historic);
        
        return Result<IReadOnlyCollection<OutputDto>>.FromSuccess(new List<OutputDto>
        {
            new()
            {
                Text = Ui("{0}, you successfully logged out in this chat in role {1} for {2}.",
                    currentRoleBind.Role.User.FirstName,
                    currentRoleBind.Role.RoleType,
                    currentRoleBind.Role.LiveEvent.Name)
            }
        });
    }
}