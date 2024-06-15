using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgClientPortRoleRepository
{
    Task AddAsync(TlgClientPortRole portRole);
    Task<IEnumerable<TlgClientPortRole>> GetAllAsync();
    Task HardDeleteAsync(TlgClientPortRole portRole);
    Task UpdateStatusAsync(TlgClientPortRole portRole, DbRecordStatus newStatus);
}