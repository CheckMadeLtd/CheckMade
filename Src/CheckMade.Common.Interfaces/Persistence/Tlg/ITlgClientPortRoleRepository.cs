using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgClientPortRoleRepository
{
    Task<IEnumerable<TlgClientPortRole>> GetAllAsync();
}