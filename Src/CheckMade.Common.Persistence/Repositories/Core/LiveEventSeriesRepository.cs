using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class LiveEventSeriesRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ILiveEventSeriesRepository
{
    
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<LiveEventSeries>> _cache = 
        Option<IReadOnlyCollection<LiveEventSeries>>.None();
    
    public Task<LiveEventSeries> GetAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<LiveEventSeries>> GetAllAsync()
    {
        if (_cache.IsNone)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (_cache.IsNone)
                {
                    const string rawQuery = """
                                            SELECT
                                            
                                            les.id AS series_id, 
                                            les.name AS series_name, 
                                            les.status AS series_status, 
                                            
                                            le.id AS live_event_id, 
                                            le.name AS live_event_name, 
                                            le.start_date AS live_event_start_date, 
                                            le.end_date AS live_event_end_date, 
                                            le.status AS live_event_status, 
                                            
                                            r.token AS role_token, 
                                            r.role_type AS role_type, 
                                            r.status AS role_status, 
                                            
                                            lev.name AS venue_name, 
                                            lev.status AS venue_status, 
                                            
                                            soa.id AS sphere_id, 
                                            soa.name AS sphere_name, 
                                            soa.details AS sphere_details, 
                                            soa.trade AS sphere_trade 
                                                
                                            FROM live_event_series les
                                            LEFT JOIN live_events le ON le.live_event_series_id = les.id  
                                            LEFT JOIN roles r ON r.live_event_id = le.id
                                            LEFT JOIN live_event_venues lev ON le.venue_id = lev.id 
                                            LEFT JOIN spheres_of_action soa ON soa.live_event_id = le.id 
                                            
                                            ORDER BY les.id, le.id, soa.id
                                            """;
                    
                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

                    var (getKey,
                        initializeModel,
                        accumulateData,
                        finalizeModel) = GetLiveEventSeriesReader();
                    
                    var liveEventSeries =
                        await ExecuteReaderOneToManyAsync(
                            command,
                            getKey,
                            initializeModel,
                            accumulateData,
                            finalizeModel);
                    
                    _cache = Option<IReadOnlyCollection<LiveEventSeries>>.Some(liveEventSeries);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cache.GetValueOrThrow();
    }

    private static (
        Func<DbDataReader, int> getKey,
        Func<DbDataReader, LiveEventSeries> initializeModel,
        Action<LiveEventSeries, DbDataReader> accumulateData,
        Func<LiveEventSeries, LiveEventSeries> finalizeModel) GetLiveEventSeriesReader()
    {
        return (
            getKey: reader => reader.GetInt32(reader.GetOrdinal("series_id")),
            
            initializeModel: reader => 
                new LiveEventSeries(
                    reader.GetString(reader.GetOrdinal("series_name")),
                    new List<LiveEvent>(),
                    EnsureEnumValidityOrThrow(
                        (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("series_status")))),
            
            accumulateData: (series, reader) => {},
            
            finalizeModel: series => 
                series with
                {
                    LiveEvents = series.LiveEvents.ToImmutableReadOnlyCollection()
                }
        );
    }
}