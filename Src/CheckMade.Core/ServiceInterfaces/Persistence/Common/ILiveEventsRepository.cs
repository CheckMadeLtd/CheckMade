using CheckMade.Core.Model.Common.LiveEvents;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Common;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}