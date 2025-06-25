using System.Collections.Concurrent;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class RolesRepository(
    IDbExecutionHelper dbHelper,
    IDomainGlossary glossary,
    RolesSharedMapper mapper) 
    : BaseRepository(dbHelper, glossary), IRolesRepository
{
    private ConcurrentDictionary<string, Task<IReadOnlyCollection<Role>>> _cache = new();
    private const string CacheKey = "all";
    
    public async Task<Role?> GetAsync(IRoleInfo role) =>
        (await GetAllAsync())
        .FirstOrDefault(r => r.Equals(role));

    public async Task<IReadOnlyCollection<Role>> GetAllAsync() =>
        await _cache.GetOrAdd(CacheKey, async _ => await LoadAllFromDbAsync());
    
    private async Task<IReadOnlyCollection<Role>> LoadAllFromDbAsync()
    {
        const string rawQuery = """
                                SELECT 

                                usr.mobile AS user_mobile, 
                                usr.first_name AS user_first_name, 
                                usr.middle_name AS user_middle_name, 
                                usr.last_name AS user_last_name, 
                                usr.email AS user_email, 
                                usr.language_setting AS user_language, 
                                usr.status AS user_status, 

                                lve.name AS live_event_name, 
                                lve.start_date AS live_event_start_date, 
                                lve.end_date AS live_event_end_date, 
                                lve.status AS live_event_status, 

                                r.id AS role_id,
                                r.token AS role_token, 
                                r.role_type AS role_type, 
                                r.status AS role_status, 

                                soa.name AS sphere_name, 
                                soa.details AS sphere_details, 
                                soa.trade AS sphere_trade,
                                soa.status AS sphere_status

                                FROM roles r 
                                INNER JOIN users usr on r.user_id = usr.id 
                                INNER JOIN live_events lve on r.live_event_id = lve.id
                                LEFT JOIN roles_to_spheres_assignments rtsa ON r.id = rtsa.role_id
                                LEFT JOIN spheres_of_action soa ON rtsa.sphere_id = soa.id

                                ORDER BY r.id, soa.id;
                                """;

        var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

        var (getKey,
            initializeModel,
            accumulateData,
            finalizeModel) = mapper.RoleMapper(Glossary);

        return await ExecuteMapperAsync(
            command, getKey, initializeModel, accumulateData, finalizeModel);
    }
}