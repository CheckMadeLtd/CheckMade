using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput);
    Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(TlgAgent tlgAgent, DateTime since);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTime since);
    Task UpdateGuid(IReadOnlyCollection<TlgInput> tlgInputs, Guid newGuid);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}