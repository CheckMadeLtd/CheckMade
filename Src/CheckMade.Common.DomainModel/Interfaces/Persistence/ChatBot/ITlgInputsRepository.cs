using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput, Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(TlgAgent tlgAgent);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(TlgAgent tlgAgent, DateTimeOffset since);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTimeOffset since);
    Task<IReadOnlyCollection<TlgInput>> GetEntityHistoryAsync(ILiveEventInfo liveEvent, Guid entityGuid);
    Task UpdateGuid(IReadOnlyCollection<TlgInput> tlgInputs, Guid newGuid);
    Task HardDeleteAllAsync(TlgAgent tlgAgent);
}