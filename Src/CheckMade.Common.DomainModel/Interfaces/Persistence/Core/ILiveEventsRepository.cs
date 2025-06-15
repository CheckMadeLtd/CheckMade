using CheckMade.Common.DomainModel.Data.Core.LiveEvents;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface ILiveEventsRepository
{
    Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent);
    Task<IReadOnlyCollection<LiveEvent>> GetAllAsync();
}