using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class LiveEventsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), ILiveEventsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<LiveEvent>> _cache = Option<IReadOnlyCollection<LiveEvent>>.None();

    private static (Func<DbDataReader, int> keyGetter, 
        Func<DbDataReader, LiveEvent> modelInitializer,
        Action<LiveEvent, DbDataReader> accumulateData,
        Func<LiveEvent, LiveEvent> modelFinalizer)
        LiveEventMapper(IDomainGlossary glossary)
    {
        return (
            keyGetter: static reader => reader.GetInt32(reader.GetOrdinal("live_event_id")),
            modelInitializer: static reader => 
                new LiveEvent(
                    ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                    new List<IRoleInfo>(),
                    ConstituteLiveEventVenue(reader),
                    new List<ISphereOfAction>()),
            accumulateData: (liveEvent, reader) =>
            {
                var accSw = System.Diagnostics.Stopwatch.StartNew();
    
                var roleInfo = ConstituteRoleInfo(reader, glossary);
    
                if (roleInfo.IsSome)
                    ((List<IRoleInfo>)liveEvent.WithRoles).Add(roleInfo.GetValueOrThrow());

                var sphereOfAction = ConstituteSphereOfAction(reader, glossary);
    
                if (sphereOfAction.IsSome)
                    ((List<ISphereOfAction>)liveEvent.DivIntoSpheres).Add(sphereOfAction.GetValueOrThrow());
    
                accSw.Stop();
                
                if (accSw.ElapsedMilliseconds > 5)
                    Console.WriteLine($"[PERF-DEBUG] Slow 'accumulateData' {accSw.ElapsedMilliseconds}ms");
            },
            modelFinalizer: static liveEvent => liveEvent with
            {
                WithRoles = liveEvent.WithRoles.Distinct().ToImmutableArray(),
                DivIntoSpheres = liveEvent.DivIntoSpheres.Distinct().ToImmutableArray()
            });
    }

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
                        finalizeModel) = LiveEventMapper(Glossary);

                    var liveEvents = await ExecuteMapperAsync(
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
}