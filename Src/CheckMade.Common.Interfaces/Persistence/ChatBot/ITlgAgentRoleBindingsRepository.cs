using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgAgentRoleBindingsRepository
{
    Task AddAsync(TlgAgentRoleBind portRoleBind);
    Task AddAsync(IEnumerable<TlgAgentRoleBind> portRole);
    Task<IEnumerable<TlgAgentRoleBind>> GetAllAsync();
    Task UpdateStatusAsync(TlgAgentRoleBind portRoleBind, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgAgentRoleBind portRoleBind);
}