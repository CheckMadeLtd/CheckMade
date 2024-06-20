using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgInputRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IEnumerable<TlgInput> tlgInputs);
    Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId);
    Task<IEnumerable<TlgInput>> GetAllAsync(TlgAgent tlgAgent);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}