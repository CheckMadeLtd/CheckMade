using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class LiveEventsRepository : ILiveEventsRepository
{
    public Task<LiveEvent> GetAsync(ILiveEventInfo liveEvent)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<LiveEvent>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}