using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface ILiveEventSeriesRepository
{
    Task<LiveEventSeries> GetAsync(string name);
    Task<IReadOnlyCollection<LiveEventSeries>> GetAllAsync();
}