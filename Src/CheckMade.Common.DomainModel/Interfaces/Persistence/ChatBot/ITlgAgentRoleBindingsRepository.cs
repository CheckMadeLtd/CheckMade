using CheckMade.Common.DomainModel.Data.ChatBot;
using CheckMade.Common.DomainModel.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;

public interface ITlgAgentRoleBindingsRepository
{
    Task AddAsync(TlgAgentRoleBind tlgAgentRoleBind);
    Task AddAsync(IReadOnlyCollection<TlgAgentRoleBind> tlgAgentRoleBindings);
    Task<IReadOnlyCollection<TlgAgentRoleBind>> GetAllAsync();
    Task<IReadOnlyCollection<TlgAgentRoleBind>> GetAllActiveAsync();
    Task UpdateStatusAsync(TlgAgentRoleBind tlgAgentRoleBind, DbRecordStatus newStatus);
    Task UpdateStatusAsync(IReadOnlyCollection<TlgAgentRoleBind> tlgAgentRoleBindings, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgAgentRoleBind tlgAgentRoleBind);
}