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
        var totalInitializerTime = 0L;
        var totalAccumulateTime = 0L;
        long totalDistinctTime;
        long totalToImmutableTime;
        var accumulateCount = 0;
        
        return (
            keyGetter: static reader => reader.GetInt32(reader.GetOrdinal("live_event_id")),
            modelInitializer: reader => 
            {
                var initSw = System.Diagnostics.Stopwatch.StartNew();
                var result = new LiveEvent(
                    ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                    new List<IRoleInfo>(),
                    ConstituteLiveEventVenue(reader),
                    new List<ISphereOfAction>());
                initSw.Stop();
                
                totalInitializerTime += initSw.ElapsedMilliseconds;
                return result;
            },
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
                totalAccumulateTime += accSw.ElapsedMilliseconds;
                accumulateCount++;
            },
            modelFinalizer: liveEvent => 
            {
                var finalizerSw = System.Diagnostics.Stopwatch.StartNew();
                
                var distinctSw = System.Diagnostics.Stopwatch.StartNew();
                var distinctRoles = liveEvent.WithRoles.Distinct();
                var distinctSpheres = liveEvent.DivIntoSpheres.Distinct();
                distinctSw.Stop();
                totalDistinctTime = distinctSw.ElapsedMilliseconds;
                
                var immutableSw = System.Diagnostics.Stopwatch.StartNew();
                var result = liveEvent with
                {
                    WithRoles = distinctRoles.ToImmutableArray(),
                    DivIntoSpheres = distinctSpheres.ToImmutableArray()
                };
                immutableSw.Stop();
                totalToImmutableTime = immutableSw.ElapsedMilliseconds;
                
                finalizerSw.Stop();
                
                Console.WriteLine($"[PERF-DEBUG] STEP BREAKDOWN - " +
                                  $"Initializer: {totalInitializerTime}ms, " +
                                  $"Accumulate: {totalAccumulateTime}ms ({accumulateCount} calls), " +
                                  $"Distinct: {totalDistinctTime}ms, " +
                                  $"ToImmutable: {totalToImmutableTime}ms, " +
                                  $"Total Finalizer: {finalizerSw.ElapsedMilliseconds}ms");
                
                return result;
            }
        );
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
                    
                    var commandSw = System.Diagnostics.Stopwatch.StartNew();
                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());
                    commandSw.Stop();
                    Console.WriteLine($"[PERF-DEBUG] GenerateCommand took: {commandSw.ElapsedMilliseconds}ms");
                
                    var (getKey,
                        initializeModel,
                        accumulateData,
                        finalizeModel) = LiveEventMapper(Glossary);

                    var executeSw = System.Diagnostics.Stopwatch.StartNew();
                    var liveEvents = await ExecuteMapperAsync(
                        command, getKey, initializeModel, accumulateData, finalizeModel);
                    executeSw.Stop();
                    Console.WriteLine($"[PERF-DEBUG] ExecuteMapperAsync took: {executeSw.ElapsedMilliseconds}ms");
                
                    var cacheSw = System.Diagnostics.Stopwatch.StartNew();
                    _cache = Option<IReadOnlyCollection<LiveEvent>>.Some(liveEvents);
                    cacheSw.Stop();
                    Console.WriteLine($"[PERF-DEBUG] Cache assignment took: {cacheSw.ElapsedMilliseconds}ms");
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