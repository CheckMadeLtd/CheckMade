using CheckMade.Abstract.Domain.Model.Common.LiveEvents;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}