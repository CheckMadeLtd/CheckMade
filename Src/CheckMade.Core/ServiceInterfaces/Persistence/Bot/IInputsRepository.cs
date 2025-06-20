using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Bot.DTOs.Output;
using CheckMade.Core.Model.Common.LiveEvents;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Bot;

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