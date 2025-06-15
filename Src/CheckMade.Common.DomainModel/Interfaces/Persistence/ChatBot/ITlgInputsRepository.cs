using CheckMade.Common.DomainModel.Data.ChatBot;
using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.Output;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

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