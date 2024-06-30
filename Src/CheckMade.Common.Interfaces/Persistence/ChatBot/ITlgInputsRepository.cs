using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs);
    Task<IReadOnlyCollection<TlgInput>> GetAllAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllAsync(ILiveEventInfo liveEvent);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}