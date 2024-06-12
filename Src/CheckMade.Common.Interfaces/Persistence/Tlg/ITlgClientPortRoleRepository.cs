using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgClientPortRoleRepository
{
    Task AddAsync(TlgClientPortRole portRole);
    Task<IEnumerable<TlgClientPortRole>> GetAllAsync();
}