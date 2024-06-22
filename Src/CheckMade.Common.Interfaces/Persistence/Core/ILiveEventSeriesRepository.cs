using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface ILiveEventSeriesRepository
{
    Task<IEnumerable<LiveEventSeries>> GetAsync(string name);
    Task<IEnumerable<LiveEventSeries>> GetAllAsync();
}