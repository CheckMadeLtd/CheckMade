using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs);
    Task<IReadOnlyCollection<TlgInput>> GetAllUserInitiatedAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllUserInitiatedAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(ILiveEventInfo liveEvent);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}