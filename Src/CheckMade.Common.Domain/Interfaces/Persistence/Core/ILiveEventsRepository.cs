using CheckMade.Common.Domain.Data.Core.LiveEvents;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}