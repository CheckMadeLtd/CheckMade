using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class LiveEventsRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ILiveEventsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<LiveEvent>> _cache = Option<IReadOnlyCollection<LiveEvent>>.None();

    public async Task<LiveEvent?> GetAsync(ILiveEventInfo liveEvent) =>
        (await GetAllAsync())
        .FirstOrDefault(le => le.Equals(liveEvent));

    public async Task<IReadOnlyCollection<LiveEvent>> GetAllAsync()
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
                                            
                                            r.token AS role_token, 
                                            r.role_type AS role_type, 
                                            r.status AS role_status,
                                            
                                            soa.name AS sphere_name, 
                                            soa.details AS sphere_details, 
                                            soa.trade AS sphere_trade, 
                                            
                                            lev.name AS venue_name, 
                                            lev.status AS venue_status, 
                                            
                                            le.id AS live_event_id, 
                                            le.name AS live_event_name, 
                                            le.start_date AS live_event_start_date, 
                                            le.end_date AS live_event_end_date, 
                                            le.status AS live_event_status 
                                            
                                            FROM live_events le
                                            LEFT JOIN roles r on r.live_event_id = le.id
                                            LEFT JOIN spheres_of_action soa on soa.live_event_id = le.id
                                            JOIN live_event_venues lev on le.venue_id = lev.id
                                            
                                            ORDER BY le.id, r.id, soa.id
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());
                    
                    var (getKey,
                        initializeModel,
                        accumulateData,
                        finalizeModel) = GetLiveEventReader();

                    var liveEvents =
                        await ExecuteReaderOneToManyAsync(
                            command, getKey, initializeModel, accumulateData, finalizeModel);
                    
                    _cache = Option<IReadOnlyCollection<LiveEvent>>.Some(liveEvents);
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
        Func<DbDataReader, LiveEvent> initializeModel,
        Action<LiveEvent, DbDataReader> accumulateData,
        Func<LiveEvent, LiveEvent> finalizeModel) GetLiveEventReader()
    {
        return (
            getKey: reader => reader.GetInt32(reader.GetOrdinal("live_event_id")),
            initializeModel: reader => 
                new LiveEvent(
                    ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                    new HashSet<IRoleInfo>(),
                    ConstituteLiveEventVenue(reader),
                    new HashSet<ISphereOfAction>()),
            accumulateData: (liveEvent, reader) =>
            {
                var roleInfo = ConstituteRoleInfo(reader);
                if (roleInfo.IsSome)
                    ((HashSet<IRoleInfo>)liveEvent.WithRoles).Add(roleInfo.GetValueOrThrow());

                var sphereOfAction = ConstituteSphereOfAction(reader);
                if (sphereOfAction.IsSome)
                    ((HashSet<ISphereOfAction>)liveEvent.DivIntoSpheres).Add(sphereOfAction.GetValueOrThrow());

            },
            finalizeModel: liveEvent => liveEvent with
            {
                WithRoles = liveEvent.WithRoles.ToImmutableReadOnlyCollection(),
                DivIntoSpheres = liveEvent.DivIntoSpheres.ToImmutableReadOnlyCollection()
            }
        );
    }
}