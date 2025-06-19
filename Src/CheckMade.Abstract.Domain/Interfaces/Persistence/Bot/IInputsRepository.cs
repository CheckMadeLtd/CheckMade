using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Data.Bot.Output;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Interfaces.Persistence.Bot;

public interface IInputsRepository
{
    Task AddAsync(Input input, Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations);
    Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(Agent agent);
    Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<Input>> GetAllLocationAsync(Agent agent, DateTimeOffset since);
    Task<IReadOnlyCollection<Input>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTimeOffset since);
    Task<IReadOnlyCollection<Input>> GetEntityHistoryAsync(ILiveEventInfo liveEvent, Guid entityGuid);
    Task UpdateGuid(IReadOnlyCollection<Input> inputs, Guid newGuid);
    Task HardDeleteAllAsync(Agent agent);
}