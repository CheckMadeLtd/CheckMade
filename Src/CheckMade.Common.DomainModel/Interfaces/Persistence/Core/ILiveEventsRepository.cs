using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}