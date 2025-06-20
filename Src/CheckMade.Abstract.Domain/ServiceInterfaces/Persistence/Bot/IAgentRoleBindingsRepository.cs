using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;

public interface IAgentRoleBindingsRepository
{
    Task AddAsync(AgentRoleBind agentRoleBind);
    Task AddAsync(IReadOnlyCollection<AgentRoleBind> agentRoleBindings);
    Task<IReadOnlyCollection<AgentRoleBind>> GetAllAsync();
    Task<IReadOnlyCollection<AgentRoleBind>> GetAllActiveAsync();
    Task UpdateStatusAsync(AgentRoleBind agentRoleBind, DbRecordStatus newStatus);
    Task UpdateStatusAsync(IReadOnlyCollection<AgentRoleBind> agentRoleBindings, DbRecordStatus newStatus);
    Task HardDeleteAsync(AgentRoleBind agentRoleBind);
}