using CheckMade.Common.Model.ChatBot;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgClientPortRoleRepository
{
    Task AddAsync(TlgClientPortRole portRole);
    Task<IEnumerable<TlgClientPortRole>> GetAllAsync();
}