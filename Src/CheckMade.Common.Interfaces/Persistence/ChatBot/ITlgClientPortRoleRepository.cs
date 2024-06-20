using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgAgentRoleRepository
{
    Task AddAsync(TlgAgentRole portRole);
    Task AddAsync(IEnumerable<TlgAgentRole> portRole);
    Task<IEnumerable<TlgAgentRole>> GetAllAsync();
    Task UpdateStatusAsync(TlgAgentRole portRole, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgAgentRole portRole);
}