using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgClientPortToRoleMapRepository
{
    Task<IEnumerable<TlgClientPortToRoleMap>> GetAllAsync();
}