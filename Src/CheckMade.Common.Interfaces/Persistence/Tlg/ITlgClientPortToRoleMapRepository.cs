using CheckMade.Common.Model.Tlg;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgClientPortToRoleMapRepository
{
    Task<IEnumerable<TlgClientPortToRoleMap>> GetAllAsync();
}