using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgAgentRoleBindingsRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgAgentRoleBindingsRepository
{
    public async Task AddAsync(TlgAgentRoleBind tlgAgentRoleBind) =>
        await AddAsync(new List<TlgAgentRoleBind> { tlgAgentRoleBind });

    public async Task AddAsync(IEnumerable<TlgAgentRoleBind> tlgAgentRole)
    {
        const string rawQuery = "INSERT INTO tlg_agent_role_bindings (" +
                                "role_id, " +
                                "tlg_user_id, " +
                                "tlg_chat_id, " +
                                "activation_date, " +
                                "deactivation_date, " +
                                "status, " +
                                "interaction_mode) " +
                                "VALUES ((SELECT id FROM roles WHERE token = @token), " +
                                "@tlgUserId, @tlgChatId, " +
                                "@activationDate, @deactivationDate, @status, @mode)";

        var commands = tlgAgentRole.Select(arb =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                { "@token", arb.Role.Token },
                { "@tlgUserId", (long)arb.TlgAgent.UserId },
                { "@tlgChatId", (long)arb.TlgAgent.ChatId },
                { "@activationDate", arb.ActivationDate },
                { "@status", (int)arb.Status },
                { "@mode", (int)arb.TlgAgent.Mode }
            };

            if (arb.DeactivationDate.IsSome)
                normalParameters.Add("@deactivationDate", arb.DeactivationDate.GetValueOrThrow());
            else
                normalParameters.Add("@deactivationDate", DBNull.Value);

            return GenerateCommand(rawQuery, normalParameters);
        });

        await ExecuteTransactionAsync(commands);
    }

    public async Task<IEnumerable<TlgAgentRoleBind>> GetAllAsync()
    {
        const string rawQuery = "SELECT " +
                                "usr.mobile AS user_mobile, " +
                                "usr.first_name AS user_first_name, " +
                                "usr.middle_name AS user_middle_name, " +
                                "usr.last_name AS user_last_name, " +
                                "usr.email AS user_email, " +
                                "usr.language_setting AS user_language, " +
                                "usr.status AS user_status, " +
                                "ven.name AS venue_name, " +
                                "ven.status AS venue_status, " +
                                "lve.name AS live_event_name, " +
                                "lve.start_date AS live_event_start_date, " +
                                "lve.end_date AS live_event_end_date, " +
                                "lve.status AS live_event_status, " +
                                "r.token AS role_token, " +
                                "r.role_type AS role_type, " +
                                "r.status AS role_status, " +
                                "tarb.tlg_user_id AS tcpr_tlg_user_id, " +
                                "tarb.tlg_chat_id AS tcpr_tlg_chat_id, " +
                                "tarb.interaction_mode AS tcpr_interaction_mode, " +
                                "tarb.activation_date AS tcpr_activation_date, " +
                                "tarb.deactivation_date AS tcpr_deactivation_date, " +
                                "tarb.status AS tcpr_status " +
                                "FROM tlg_agent_role_bindings tarb " +
                                "INNER JOIN roles r on tarb.role_id = r.id " +
                                "INNER JOIN users usr on r.user_id = usr.id " +
                                "INNER JOIN live_events lve on r.live_event_id = lve.id " +
                                "INNER JOIN live_event_venues ven on lve.venue_id = ven.id";

        var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

        return await ExecuteReaderAsync(command, reader =>
        {
            var user = new User(
                new MobileNumber(reader.GetString(reader.GetOrdinal("user_mobile"))),
                reader.GetString(reader.GetOrdinal("user_first_name")),
                GetOption<string>(reader, reader.GetOrdinal("user_middle_name")),
                reader.GetString(reader.GetOrdinal("user_last_name")),
                GetOption<EmailAddress>(reader, reader.GetOrdinal("user_email")),
                (LanguageCode)reader.GetInt16(reader.GetOrdinal("user_language")),
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("user_status")));

            var venue = new LiveEventVenue(
                reader.GetString(reader.GetOrdinal("venue_name")),
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("venue_status")));
            
            var liveEvent = new LiveEvent(
                reader.GetString(reader.GetOrdinal("live_event_name")),
                reader.GetDateTime(reader.GetOrdinal("live_event_start_date")),
                reader.GetDateTime(reader.GetOrdinal("live_event_end_date")),
                new List<Role>(),
                venue,
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("live_event_status")));
            
            var role = new Role(
                reader.GetString(reader.GetOrdinal("role_token")),
                (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
                user,
                liveEvent,
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status")));
                    
            var tlgAgent = new TlgAgent(
                reader.GetInt64(reader.GetOrdinal("tcpr_tlg_user_id")),
                reader.GetInt64(reader.GetOrdinal("tcpr_tlg_chat_id")),
                (InteractionMode)reader.GetInt16(reader.GetOrdinal("tcpr_interaction_mode")));

            var activationDate = reader.GetDateTime(reader.GetOrdinal("tcpr_activation_date"));

            var deactivationDateOrdinal = reader.GetOrdinal("tcpr_deactivation_date");
            
            var deactivationDate = !reader.IsDBNull(deactivationDateOrdinal) 
                ? Option<DateTime>.Some(reader.GetDateTime(deactivationDateOrdinal)) 
                : Option<DateTime>.None();
                    
            var status = (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("tcpr_status"));

            return new TlgAgentRoleBind(role, tlgAgent, activationDate, deactivationDate, status);
        });
    }

    public async Task UpdateStatusAsync(TlgAgentRoleBind tlgAgentRoleBind, DbRecordStatus newStatus)
    {
        const string rawQuery = "UPDATE tlg_agent_role_bindings " +
                                "SET status = @status, deactivation_date = @deactivationDate " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId " + 
                                "AND interaction_mode = @mode";
    
        var normalParameters = new Dictionary<string, object>
        {
            { "@status", (int)newStatus },
            { "@token", tlgAgentRoleBind.Role.Token },
            { "@tlgUserId", (long)tlgAgentRoleBind.TlgAgent.UserId },
            { "@tlgChatId", (long)tlgAgentRoleBind.TlgAgent.ChatId },
            { "@mode", (int)tlgAgentRoleBind.TlgAgent.Mode }
        };

        if (newStatus != DbRecordStatus.Active)
            normalParameters.Add("@deactivationDate", DateTime.UtcNow);
        else
            normalParameters.Add("@deactivationDate", DBNull.Value);
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
    
    public async Task HardDeleteAsync(TlgAgentRoleBind tlgAgentRoleBind)
    {
        const string rawQuery = "DELETE FROM tlg_agent_role_bindings " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId " +
                                "AND interaction_mode = @mode";
        
        var normalParameters = new Dictionary<string, object>
        {
            { "@token", tlgAgentRoleBind.Role.Token },
            { "tlgUserId", (long)tlgAgentRoleBind.TlgAgent.UserId },
            { "tlgChatId", (long)tlgAgentRoleBind.TlgAgent.ChatId },
            { "@mode", (int)tlgAgentRoleBind.TlgAgent.Mode }
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}