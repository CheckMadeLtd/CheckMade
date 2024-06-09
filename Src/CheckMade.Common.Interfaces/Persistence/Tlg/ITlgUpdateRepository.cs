using CheckMade.Common.Model.Tlg.Updates;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgUpdateRepository
{
    Task AddAsync(TlgUpdate tlgUpdate);
    Task AddAsync(IEnumerable<TlgUpdate> tlgUpdates);
    Task<IEnumerable<TlgUpdate>> GetAllAsync();
    Task<IEnumerable<TlgUpdate>> GetAllAsync(TlgUserId userId);
    Task HardDeleteAllAsync(TlgUserId userId);
}