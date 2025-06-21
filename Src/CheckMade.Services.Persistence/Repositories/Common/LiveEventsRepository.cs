using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.Logging;
using static CheckMade.Services.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class LiveEventsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary, 
    ILogger<LiveEventsRepository> logger) 
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
        var accumulateCallCount = 0;
        var roleAddCount = 0;
        var sphereAddCount = 0;
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var totalRoleConstituteTime = 0L;
        var totalSphereConstituteTime = 0L;
        var totalAccumulateTime = 0L;
        
        return (
            keyGetter: static reader => reader.GetInt32(reader.GetOrdinal("live_event_id")),
            modelInitializer: static reader => 
            {
                var initStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = new LiveEvent(
                    ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                    new List<IRoleInfo>(),
                    ConstituteLiveEventVenue(reader),
                    new List<ISphereOfAction>());
                initStopwatch.Stop();
                
                if (initStopwatch.ElapsedMilliseconds > 10)
                    Console.WriteLine($"[PERF-DEBUG] ModelInitializer took {initStopwatch.ElapsedMilliseconds}ms");
                
                return result;
            },
            accumulateData: (liveEvent, reader) =>
            {
                var accStopwatch = System.Diagnostics.Stopwatch.StartNew();
                accumulateCallCount++;
                
                var roleStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var roleInfo = ConstituteRoleInfo(reader, glossary);
                roleStopwatch.Stop();
                totalRoleConstituteTime += roleStopwatch.ElapsedMilliseconds;
                
                if (roleInfo.IsSome)
                {
                    roleAddCount++;
                    ((List<IRoleInfo>)liveEvent.WithRoles).Add(roleInfo.GetValueOrThrow());
                }

                var sphereStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var sphereOfAction = ConstituteSphereOfAction(reader, glossary);
                sphereStopwatch.Stop();
                totalSphereConstituteTime += sphereStopwatch.ElapsedMilliseconds;
                
                if (sphereOfAction.IsSome)
                {
                    sphereAddCount++;
                    ((List<ISphereOfAction>)liveEvent.DivIntoSpheres).Add(sphereOfAction.GetValueOrThrow());
                }
                
                accStopwatch.Stop();
                totalAccumulateTime += accStopwatch.ElapsedMilliseconds;
            },
            modelFinalizer: liveEvent => 
            {
                var finalStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = liveEvent with
                {
                    WithRoles = liveEvent.WithRoles.ToImmutableArray(),
                    DivIntoSpheres = liveEvent.DivIntoSpheres.ToImmutableArray()
                };
                finalStopwatch.Stop();
                totalStopwatch.Stop();
                
                Console.WriteLine($"[PERF-DEBUG] FINAL STATS: {accumulateCallCount} accumulate calls, " +
                                  $"{roleAddCount} roles, {sphereAddCount} spheres. " +
                                  $"Role constitute total: {totalRoleConstituteTime}ms, " +
                                  $"Sphere constitute total: {totalSphereConstituteTime}ms, " +
                                  $"Accumulate total: {totalAccumulateTime}ms, " +
                                  $"Finalizer: {finalStopwatch.ElapsedMilliseconds}ms, " +
                                  $"Total mapper time: {totalStopwatch.ElapsedMilliseconds}ms");
                
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

                    
                    // // TEMPORARY DEBUG: First run EXPLAIN ANALYZE to capture execution plan
                    // const string explainQuery = "EXPLAIN ANALYZE " + rawQuery;
                    // var explainCommand = GenerateCommand(explainQuery, Option<Dictionary<string, object>>.None());
                    //
                    // var explainResults = 
                    //     await ExecuteMapperAsync(
                    //         explainCommand, static (reader, _) => reader.GetString(0));
                    //
                    // // Log the execution plan to Application Insights
                    // var formattedExplainOutput = string.Join(" || ", explainResults)
                    //     .Replace('\n', ' ').Replace('\r', ' ');
                    //
                    // logger.LogDebug("EXPLAIN ANALYZE output: {ExplainPlan}", 
                    //     formattedExplainOutput);                    
                    
                    
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