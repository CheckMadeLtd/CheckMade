using CheckMade.Abstract.Domain.Model.Core.LiveEvents;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}