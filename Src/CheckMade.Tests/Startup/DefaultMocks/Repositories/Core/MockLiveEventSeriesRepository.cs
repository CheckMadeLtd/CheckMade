using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Utils;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;

public class MockLiveEventSeriesRepository : ILiveEventSeriesRepository
{
    private readonly ImmutableList<LiveEventSeries> _allSeries =
        [
            ..new List<LiveEventSeries>
            {
                TestData.MockParookaSeries,
                TestData.MockHurricaneSeries
            }
        ];
    
    public Task<IEnumerable<LiveEventSeries>> GetAsync(string name)
    {
        return Task.FromResult(_allSeries.Where(s => s.Name == name));
    }

    public Task<IEnumerable<LiveEventSeries>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<LiveEventSeries>>(_allSeries);
    }
}