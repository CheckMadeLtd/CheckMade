using CheckMade.Common.Model.Tlg.Input;

namespace CheckMade.Common.Interfaces.Persistence.Tlg;

public interface ITlgInputRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IEnumerable<TlgInput> tlgInputs);
    Task<IEnumerable<TlgInput>> GetAllAsync();
    Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId);
    Task HardDeleteAllAsync(TlgUserId userId);
}