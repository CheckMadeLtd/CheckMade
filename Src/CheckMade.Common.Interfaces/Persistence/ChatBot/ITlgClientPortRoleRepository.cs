using CheckMade.Common.Model.ChatBot;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgClientPortRoleRepository
{
    Task AddAsync(TlgClientPortRole portRole);
    Task<IEnumerable<TlgClientPortRole>> GetAllAsync();
    Task HardDeleteAsync(TlgClientPortRole portRole);
}