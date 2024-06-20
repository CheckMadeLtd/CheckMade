using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgTlgAgentRoleRepository
{
    Task AddAsync(TlgTlgAgentRole portRole);
    Task AddAsync(IEnumerable<TlgTlgAgentRole> portRole);
    Task<IEnumerable<TlgTlgAgentRole>> GetAllAsync();
    Task UpdateStatusAsync(TlgTlgAgentRole portRole, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgTlgAgentRole portRole);
}