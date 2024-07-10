using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}