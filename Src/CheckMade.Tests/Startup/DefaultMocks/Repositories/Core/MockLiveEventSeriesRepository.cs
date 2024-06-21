using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;

public class MockLiveEventSeriesRepository : ILiveEventSeriesRepository
{
    public Task<IEnumerable<LiveEventSeries>> GetAsync(string name)
    {
        var builder = ImmutableArray.CreateBuilder<LiveEventSeries>();

        var mockParookaVenue = new LiveEventVenue("Mock Venue near Cologne");
        var mockHurricaneVenue = new LiveEventVenue("Mock Venue near Bremen");

        builder.AddRange(

            new LiveEventSeries("Mock Parookaville",
                new List<LiveEvent>
                {
                    new("Mock Parookaville 2024",
                        new DateTime(2024, 07, 19, 10, 00, 00, DateTimeKind.Utc),
                        new DateTime(2024, 07, 22, 18, 00, 00, DateTimeKind.Utc),
                        new List<Role>
                        {
                            ITestUtils.SanitaryOpsAdmin1AtMockParooka2024
                        },
                        mockParookaVenue),

                    new("Mock Parookaville 2025",
                        new DateTime(2025, 07, 18, 10, 00, 00, DateTimeKind.Utc),
                        new DateTime(2025, 07, 21, 18, 00, 00, DateTimeKind.Utc),
                        new List<Role>(),
                        mockParookaVenue)
                }),

            new LiveEventSeries("Mock Hurricane",
                new List<LiveEvent>
                {
                    new("Mock Hurricane 2024",
                        new DateTime(2024, 06, 21, 10, 00, 00, DateTimeKind.Utc),
                        new DateTime(2024, 06, 24, 18, 00, 00, DateTimeKind.Utc),
                        new List<Role>
                        {
                            ITestUtils.SanitaryOpsInspector2AtMockHurricane2024
                        },
                        mockHurricaneVenue),
                    
                    new("Mock Hurricane 2025",
                        new DateTime(2025, 06, 20, 10, 00, 00, DateTimeKind.Utc),
                        new DateTime(2025, 06, 23, 18, 00, 00, DateTimeKind.Utc),
                        new List<Role>(),
                        mockHurricaneVenue)
                })
        );
        
        return Task.FromResult<IEnumerable<LiveEventSeries>>(builder.ToImmutable());
    }

    public Task<IEnumerable<LiveEventSeries>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}