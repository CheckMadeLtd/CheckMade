using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using General.Utils.FpExtensions.Monads;
using CheckMade.Services.Persistence.Repositories.Core;
using static CheckMade.Services.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Services.Persistence.Repositories.ChatBot;

public sealed class AgentRoleBindingsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), IAgentRoleBindingsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<AgentRoleBind>> _cache = Option<IReadOnlyCollection<AgentRoleBind>>.None();

    private static (Func<DbDataReader, int> keyGetter,
        Func<DbDataReader, AgentRoleBind> modelInitializer,
        Action<AgentRoleBind, DbDataReader> accumulateData,
        Func<AgentRoleBind, AgentRoleBind> modelFinalizer)
        AgentRoleBindMapper(IDomainGlossary glossary)
    {
        return (
            keyGetter: static reader => reader.GetInt32(reader.GetOrdinal("arb_id")),
            modelInitializer: reader =>
            {
                var role = RolesRepository.CreateRoleWithoutSphereAssignments(reader, glossary);
                var agent = ConstituteAgent(reader);
                
                return ConstituteAgentRoleBind(reader, role, agent);
            },
            accumulateData: (arb, reader) => 
                RolesRepository.AccumulateSphereAssignments(arb.Role, reader, glossary),
            modelFinalizer: static arb => arb with 
            { 
                Role = RolesRepository.FinalizeSphereAssignments(arb.Role)
            }
        );
    }

    public async Task AddAsync(AgentRoleBind agentRoleBind) =>
        await AddAsync([agentRoleBind]);

    public async Task AddAsync(IReadOnlyCollection<AgentRoleBind> agentRoleBindings)
    {
        const string rawQuery = """
                                INSERT INTO agent_role_bindings (

                                role_id, 
                                user_id, 
                                chat_id, 
                                activation_date, 
                                deactivation_date, 
                                status, 
                                interaction_mode) 
                                
                                VALUES ((SELECT id FROM roles WHERE token = @token), 
                                @userId, @chatId, 
                                @activationDate, @deactivationDate, @status, @mode)
                                """;

        var commands = agentRoleBindings.Select(static arb =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@token"] = arb.Role.Token,
                ["@userId"] = (long)arb.Agent.UserId,
                ["@chatId"] = (long)arb.Agent.ChatId,
                ["@activationDate"] = arb.ActivationDate,
                ["@status"] = (int)arb.Status,
                ["@mode"] = (int)arb.Agent.Mode,
                ["@deactivationDate"] = arb.DeactivationDate.Match<object>(
                    static date => date,
                    static () => DBNull.Value)
            };

            return GenerateCommand(rawQuery, normalParameters);
        });

        await ExecuteTransactionAsync(commands.ToArray());
        
        _cache = _cache.Match(
            cache => Option<IReadOnlyCollection<AgentRoleBind>>.Some(
                cache.Concat(agentRoleBindings).ToArray()),
            Option<IReadOnlyCollection<AgentRoleBind>>.None);
    }

    public async Task<IReadOnlyCollection<AgentRoleBind>> GetAllAsync()
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

                                            arb.id AS arb_id,
                                            arb.user_id AS arb_user_id, 
                                            arb.chat_id AS arb_chat_id, 
                                            arb.interaction_mode AS arb_interaction_mode, 
                                            arb.activation_date AS arb_activation_date, 
                                            arb.deactivation_date AS arb_deactivation_date, 
                                            arb.status AS arb_status,
                                            
                                            soa.name AS sphere_name, 
                                            soa.details AS sphere_details, 
                                            soa.trade AS sphere_trade,
                                            soa.status AS sphere_status

                                            FROM agent_role_bindings arb 
                                            INNER JOIN roles r on arb.role_id = r.id 
                                            INNER JOIN users usr on r.user_id = usr.id 
                                            INNER JOIN live_events lve on r.live_event_id = lve.id
                                            LEFT JOIN roles_to_spheres_assignments rtsa ON r.id = rtsa.role_id
                                            LEFT JOIN spheres_of_action soa ON rtsa.sphere_id = soa.id
                                            
                                            ORDER BY arb.id
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

                    var (getKey,
                        initializeModel,
                        accumulateData,
                        finalizeModel) = AgentRoleBindMapper(Glossary);
                    
                    var fetchedBindings = await ExecuteMapperAsync(
                        command, getKey, initializeModel, accumulateData, finalizeModel);
                    
                    _cache = Option<IReadOnlyCollection<AgentRoleBind>>.Some(
                        fetchedBindings.ToArray());
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
        
        return _cache.GetValueOrThrow();
    }

    public async Task<IReadOnlyCollection<AgentRoleBind>> GetAllActiveAsync() =>
        (await GetAllAsync())
        .Where(static arb => arb.Status.Equals(DbRecordStatus.Active))
        .ToImmutableArray();

    public async Task UpdateStatusAsync(AgentRoleBind agentRoleBind, DbRecordStatus newStatus) =>
        await UpdateStatusAsync([agentRoleBind], newStatus);

    public async Task UpdateStatusAsync(
        IReadOnlyCollection<AgentRoleBind> agentRoleBindings, 
        DbRecordStatus newStatus)
    {
        const string rawQuery = """
                                UPDATE agent_role_bindings 
                                
                                SET status = @newStatus, deactivation_date = @deactivationDate 
                                
                                WHERE role_id = (SELECT id FROM roles WHERE token = @token) 
                                AND user_id = @userId 
                                AND chat_id = @chatId  
                                AND interaction_mode = @mode 
                                AND status = @oldStatus
                                """;

        var commands = agentRoleBindings.Select(arb =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@newStatus"] = (int)newStatus,
                ["@token"] = arb.Role.Token,
                ["@userId"] = (long)arb.Agent.UserId,
                ["@chatId"] = (long)arb.Agent.ChatId,
                ["@mode"] = (int)arb.Agent.Mode,
                ["@oldStatus"] = (int)arb.Status
            };

            if (newStatus != DbRecordStatus.Active)
                normalParameters.Add("@deactivationDate", DateTimeOffset.UtcNow);
            else
                normalParameters.Add("@deactivationDate", DBNull.Value);
        
            return GenerateCommand(rawQuery, normalParameters);
        });
        
        await ExecuteTransactionAsync(commands.ToArray());
        EmptyCache();
    }

    public async Task HardDeleteAsync(AgentRoleBind agentRoleBind)
    {
        const string rawQuery = """
                                DELETE FROM agent_role_bindings 
                                       
                                WHERE role_id = (SELECT id FROM roles WHERE token = @token) 
                                AND user_id = @userId 
                                AND chat_id = @chatId 
                                AND interaction_mode = @mode
                                """;
        
        var normalParameters = new Dictionary<string, object>
        {
            ["@token"] = agentRoleBind.Role.Token,
            ["userId"] = (long)agentRoleBind.Agent.UserId,
            ["chatId"] = (long)agentRoleBind.Agent.ChatId,
            ["@mode"] = (int)agentRoleBind.Agent.Mode
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync([command]);
        EmptyCache();
    }

    private void EmptyCache() => _cache = Option<IReadOnlyCollection<AgentRoleBind>>.None();
}