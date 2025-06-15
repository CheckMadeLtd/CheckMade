using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;

public interface ITlgInputsRepository
{
    Task AddAsync(TlgInput tlgInput, Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(Agent agent);
    Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(Agent agent, DateTimeOffset since);
    Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTimeOffset since);
    Task<IReadOnlyCollection<TlgInput>> GetEntityHistoryAsync(ILiveEventInfo liveEvent, Guid entityGuid);
    Task UpdateGuid(IReadOnlyCollection<TlgInput> tlgInputs, Guid newGuid);
    Task HardDeleteAllAsync(Agent agent);
}