using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class RolesRepository(
    IDbExecutionHelper dbHelper,
    IDomainGlossary glossary,
    DomainModelConstitutors constitutors) 
    : BaseRepository(dbHelper, glossary), IRolesRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private Option<IReadOnlyCollection<Role>> _cache = Option<IReadOnlyCollection<Role>>.None();

    private static readonly Func<DbDataReader, int> GetRoleKey = 
        static reader => reader.GetInt32(reader.GetOrdinal("role_id"));

    public Func<DbDataReader, IDomainGlossary, Role> CreateRoleWithoutSphereAssignments { get; } =
        static (reader, glossary) =>
            new Role(
                ConstituteRoleInfo(reader, glossary).GetValueOrThrow(),
                ConstituteUserInfo(reader),
                ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                new HashSet<ISphereOfAction>()
            );

    public Action<Role, DbDataReader> GetAccumulateSphereAssignments(IDomainGlossary glossary) =>
        (role, reader) =>
        {
            var assignedSphere = constitutors.ConstituteSphereOfAction(reader, glossary);
            if (assignedSphere.IsSome)
                ((HashSet<ISphereOfAction>)role.AssignedToSpheres).Add(assignedSphere.GetValueOrThrow());
        };
    
    public Func<Role, Role> FinalizeSphereAssignments { get; } = 
        static role => role with
        {
            AssignedToSpheres = role.AssignedToSpheres.ToImmutableArray()
        };

    private (Func<DbDataReader, int> keyGetter,
        Func<DbDataReader, Role> modelInitializer,
        Action<Role, DbDataReader> accumulateData,
        Func<Role, Role> modelFinalizer)
        RoleMapper(IDomainGlossary glossary)
    {
        return (
            keyGetter: GetRoleKey,
            modelInitializer: reader => CreateRoleWithoutSphereAssignments(reader, glossary),
            accumulateData: GetAccumulateSphereAssignments(glossary),
            modelFinalizer: FinalizeSphereAssignments
        );
    }
    
    public async Task<Role?> GetAsync(IRoleInfo role) =>
        (await GetAllAsync())
        .FirstOrDefault(r => r.Equals(role));

    public async Task<IReadOnlyCollection<Role>> GetAllAsync()
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
                        finalizeModel) = RoleMapper(Glossary);

                    var roles = await ExecuteMapperAsync(
                        command, getKey, initializeModel, accumulateData, finalizeModel);
                    
                    _cache = Option<IReadOnlyCollection<Role>>.Some(roles);
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