using CheckMade.Abstract.Domain.Data.Core.LiveEvents;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;

namespace CheckMade.Abstract.Domain.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}