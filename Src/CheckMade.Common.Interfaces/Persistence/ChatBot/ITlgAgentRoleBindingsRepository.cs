using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgAgentRoleBindingsRepository
{
    Task AddAsync(TlgAgentRoleBind tlgAgentRoleBind);
    Task AddAsync(IEnumerable<TlgAgentRoleBind> tlgAgentRole);
    Task<IEnumerable<TlgAgentRoleBind>> GetAllAsync();
    Task UpdateStatusAsync(TlgAgentRoleBind tlgAgentRoleBind, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgAgentRoleBind tlgAgentRoleBind);
}