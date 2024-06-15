using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgClientPortRoleRepository
{
    Task AddAsync(TlgClientPortModeRole portModeRole);
    Task<IEnumerable<TlgClientPortModeRole>> GetAllAsync();
    Task UpdateStatusAsync(TlgClientPortModeRole portModeRole, DbRecordStatus newStatus);
    Task HardDeleteAsync(TlgClientPortModeRole portModeRole);
}