using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}