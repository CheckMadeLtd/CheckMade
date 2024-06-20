using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgClientPortRoleRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgClientPortRoleRepository
{
    public async Task AddAsync(TlgClientPortRole portRole) =>
        await AddAsync(new List<TlgClientPortRole> { portRole });

    public async Task AddAsync(IEnumerable<TlgClientPortRole> portRole)
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

        var commands = portRole.Select(cpr =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                { "@token", cpr.Role.Token },
                { "@tlgUserId", (long)cpr.ClientPort.UserId },
                { "@tlgChatId", (long)cpr.ClientPort.ChatId },
                { "@activationDate", cpr.ActivationDate },
                { "@status", (int)cpr.Status },
                { "@mode", (int)cpr.ClientPort.Mode }
            };

            if (cpr.DeactivationDate.IsSome)
                normalParameters.Add("@deactivationDate", cpr.DeactivationDate.GetValueOrThrow());
            else
                normalParameters.Add("@deactivationDate", DBNull.Value);

            return GenerateCommand(rawQuery, normalParameters);
        });

        await ExecuteTransactionAsync(commands);
    }

    public async Task<IEnumerable<TlgClientPortRole>> GetAllAsync()
    {
        const string rawQuery = "SELECT " +
                                "usr.mobile AS user_mobile, " +
                                "usr.first_name AS user_first_name, " +
                                "usr.middle_name AS user_middle_name, " +
                                "usr.last_name AS user_last_name, " +
                                "usr.email AS user_email, " +
                                "usr.language_setting AS user_language, " +
                                "usr.status AS user_status, " +
                                "r.token AS role_token, " +
                                "r.role_type AS role_type, " +
                                "r.status AS role_status, " +
                                "tcpr.tlg_user_id AS tcpr_tlg_user_id, " +
                                "tcpr.tlg_chat_id AS tcpr_tlg_chat_id, " +
                                "tcpr.interaction_mode AS tcpr_interaction_mode, " +
                                "tcpr.activation_date AS tcpr_activation_date, " +
                                "tcpr.deactivation_date AS tcpr_deactivation_date, " +
                                "tcpr.status AS tcpr_status " +
                                "FROM tlg_agent_role_bindings tcpr " +
                                "INNER JOIN roles r on tcpr.role_id = r.id " +
                                "INNER JOIN users usr on r.user_id = usr.id";

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

            var role = new Role(
                reader.GetString(reader.GetOrdinal("role_token")),
                (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
                user,
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

            return new TlgClientPortRole(role, tlgAgent, activationDate, deactivationDate, status);
        });
    }

    public async Task UpdateStatusAsync(TlgClientPortRole portRole, DbRecordStatus newStatus)
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
            { "@token", portRole.Role.Token },
            { "@tlgUserId", (long)portRole.ClientPort.UserId },
            { "@tlgChatId", (long)portRole.ClientPort.ChatId },
            { "@mode", (int)portRole.ClientPort.Mode }
        };

        if (newStatus != DbRecordStatus.Active)
            normalParameters.Add("@deactivationDate", DateTime.UtcNow);
        else
            normalParameters.Add("@deactivationDate", DBNull.Value);
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
    
    public async Task HardDeleteAsync(TlgClientPortRole portRole)
    {
        const string rawQuery = "DELETE FROM tlg_agent_role_bindings " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId " +
                                "AND interaction_mode = @mode";
        
        var normalParameters = new Dictionary<string, object>
        {
            { "@token", portRole.Role.Token },
            { "tlgUserId", (long)portRole.ClientPort.UserId },
            { "tlgChatId", (long)portRole.ClientPort.ChatId },
            { "@mode", (int)portRole.ClientPort.Mode }
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}