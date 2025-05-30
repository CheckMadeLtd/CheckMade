using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public sealed class TlgAgentRoleBindingsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), ITlgAgentRoleBindingsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<TlgAgentRoleBind>> _cache = Option<IReadOnlyCollection<TlgAgentRoleBind>>.None();
    
    public async Task AddAsync(TlgAgentRoleBind tlgAgentRoleBind) =>
        await AddAsync([tlgAgentRoleBind]);

    public async Task AddAsync(IReadOnlyCollection<TlgAgentRoleBind> tlgAgentRoleBindings)
    {
        const string rawQuery = """
                                INSERT INTO tlg_agent_role_bindings (

                                role_id, 
                                tlg_user_id, 
                                tlg_chat_id, 
                                activation_date, 
                                deactivation_date, 
                                status, 
                                interaction_mode) 
                                
                                VALUES ((SELECT id FROM roles WHERE token = @token), 
                                @tlgUserId, @tlgChatId, 
                                @activationDate, @deactivationDate, @status, @mode)
                                """;

        var commands = tlgAgentRoleBindings.Select(static tarb =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@token"] = tarb.Role.Token,
                ["@tlgUserId"] = (long)tarb.TlgAgent.UserId,
                ["@tlgChatId"] = (long)tarb.TlgAgent.ChatId,
                ["@activationDate"] = tarb.ActivationDate,
                ["@status"] = (int)tarb.Status,
                ["@mode"] = (int)tarb.TlgAgent.Mode,
                ["@deactivationDate"] = tarb.DeactivationDate.Match<object>(
                    static date => date,
                    static () => DBNull.Value)
            };

            return GenerateCommand(rawQuery, normalParameters);
        });

        await ExecuteTransactionAsync(commands.ToArray());
        
        _cache = _cache.Match(
            cache => Option<IReadOnlyCollection<TlgAgentRoleBind>>.Some(
                cache.Concat(tlgAgentRoleBindings).ToArray()),
            Option<IReadOnlyCollection<TlgAgentRoleBind>>.None);
    }

    public async Task<IReadOnlyCollection<TlgAgentRoleBind>> GetAllAsync()
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

                                            r.token AS role_token, 
                                            r.role_type AS role_type, 
                                            r.status AS role_status, 

                                            tarb.tlg_user_id AS tarb_tlg_user_id, 
                                            tarb.tlg_chat_id AS tarb_tlg_chat_id, 
                                            tarb.interaction_mode AS tarb_interaction_mode, 
                                            tarb.activation_date AS tarb_activation_date, 
                                            tarb.deactivation_date AS tarb_deactivation_date, 
                                            tarb.status AS tarb_status 

                                            FROM tlg_agent_role_bindings tarb 
                                            INNER JOIN roles r on tarb.role_id = r.id 
                                            INNER JOIN users usr on r.user_id = usr.id 
                                            INNER JOIN live_events lve on r.live_event_id = lve.id 
                                            
                                            ORDER BY tarb.id
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

                    var fetchedBindings = new List<TlgAgentRoleBind>(
                        await ExecuteReaderOneToOneAsync(
                            command, ModelReaders.ReadTlgAgentRoleBind));
                    
                    _cache = Option<IReadOnlyCollection<TlgAgentRoleBind>>.Some(
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

    public async Task<IReadOnlyCollection<TlgAgentRoleBind>> GetAllActiveAsync() =>
        (await GetAllAsync())
        .Where(static tarb => tarb.Status.Equals(DbRecordStatus.Active))
        .ToImmutableArray();

    public async Task UpdateStatusAsync(TlgAgentRoleBind tlgAgentRoleBind, DbRecordStatus newStatus) =>
        await UpdateStatusAsync([tlgAgentRoleBind], newStatus);

    public async Task UpdateStatusAsync(
        IReadOnlyCollection<TlgAgentRoleBind> tlgAgentRoleBindings, 
        DbRecordStatus newStatus)
    {
        const string rawQuery = """
                                UPDATE tlg_agent_role_bindings 
                                
                                SET status = @newStatus, deactivation_date = @deactivationDate 
                                
                                WHERE role_id = (SELECT id FROM roles WHERE token = @token) 
                                AND tlg_user_id = @tlgUserId 
                                AND tlg_chat_id = @tlgChatId  
                                AND interaction_mode = @mode 
                                AND status = @oldStatus
                                """;

        var commands = tlgAgentRoleBindings.Select(tarb =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@newStatus"] = (int)newStatus,
                ["@token"] = tarb.Role.Token,
                ["@tlgUserId"] = (long)tarb.TlgAgent.UserId,
                ["@tlgChatId"] = (long)tarb.TlgAgent.ChatId,
                ["@mode"] = (int)tarb.TlgAgent.Mode,
                ["@oldStatus"] = (int)tarb.Status
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

    public async Task HardDeleteAsync(TlgAgentRoleBind tlgAgentRoleBind)
    {
        const string rawQuery = """
                                DELETE FROM tlg_agent_role_bindings 
                                       
                                WHERE role_id = (SELECT id FROM roles WHERE token = @token) 
                                AND tlg_user_id = @tlgUserId 
                                AND tlg_chat_id = @tlgChatId 
                                AND interaction_mode = @mode
                                """;
        
        var normalParameters = new Dictionary<string, object>
        {
            ["@token"] = tlgAgentRoleBind.Role.Token,
            ["tlgUserId"] = (long)tlgAgentRoleBind.TlgAgent.UserId,
            ["tlgChatId"] = (long)tlgAgentRoleBind.TlgAgent.ChatId,
            ["@mode"] = (int)tlgAgentRoleBind.TlgAgent.Mode
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync([command]);
        EmptyCache();
    }

    private void EmptyCache() => _cache = Option<IReadOnlyCollection<TlgAgentRoleBind>>.None();
}