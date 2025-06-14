using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;

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