using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgClientPortModeRoleRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgClientPortModeRoleRepository
{
    public async Task AddAsync(TlgClientPortModeRole portModeRole) =>
        await AddAsync(new List<TlgClientPortModeRole> { portModeRole });

    public async Task AddAsync(IEnumerable<TlgClientPortModeRole> portModeRole)
    {
        const string rawQuery = "INSERT INTO tlg_client_port_mode_roles (" +
                                "role_id, tlg_user_id, tlg_chat_id, activation_date, " +
                                "deactivation_date, status, interaction_mode) " +
                                "VALUES ((SELECT id FROM roles WHERE token = @token), @tlgUserId, @tlgChatId, " +
                                "@activationDate, @deactivationDate, @status, @mode)";

        var commands = portModeRole.Select(cpmr =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                { "@token", cpmr.Role.Token },
                { "@tlgUserId", (long)cpmr.ClientPort.UserId },
                { "@tlgChatId", (long)cpmr.ClientPort.ChatId },
                { "@activationDate", cpmr.ActivationDate },
                { "@status", (int)cpmr.Status },
                { "@mode", (int)cpmr.ClientPort.Mode }
            };

            if (cpmr.DeactivationDate.IsSome)
                normalParameters.Add("@deactivationDate", cpmr.DeactivationDate.GetValueOrThrow());
            else
                normalParameters.Add("@deactivationDate", DBNull.Value);

            return GenerateCommand(rawQuery, normalParameters);
        });

        await ExecuteTransactionAsync(commands);
    }

    public async Task<IEnumerable<TlgClientPortModeRole>> GetAllAsync()
    {
        const string rawQuery = "SELECT r.token, r.role_type, r.status, tcpmr.tlg_user_id, tcpmr.tlg_chat_id, " +
                                "tcpmr.interaction_mode, tcpmr.activation_date, tcpmr.deactivation_date, tcpmr.status " +
                                "FROM tlg_client_port_mode_roles tcpmr " +
                                "JOIN roles r on tcpmr.role_id = r.id";

        var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

        return await ExecuteReaderAsync(command, reader =>
        {
            var role = new Role(
                reader.GetString(0),
                (RoleType)reader.GetInt16(1),
                (DbRecordStatus)reader.GetInt16(2));
                    
            var clientPort = new TlgClientPort(
                reader.GetInt64(3),
                reader.GetInt64(4),
                (InteractionMode)reader.GetInt16(5));

            var activationDate = reader.GetDateTime(6);
                    
            var deactivationDate = !reader.IsDBNull(7) 
                ? Option<DateTime>.Some(reader.GetDateTime(7)) 
                : Option<DateTime>.None();
                    
            var status = (DbRecordStatus)reader.GetInt16(8);

            return new TlgClientPortModeRole(role, clientPort, activationDate, deactivationDate, status);
        });
    }

    public async Task UpdateStatusAsync(TlgClientPortModeRole portModeRole, DbRecordStatus newStatus)
    {
        const string rawQuery = "UPDATE tlg_client_port_mode_roles " +
                                "SET status = @status, deactivation_date = @date " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId " + 
                                "AND interaction_mode = @mode";
    
        var normalParameters = new Dictionary<string, object>
        {
            { "@status", (int)newStatus },
            { "@token", portModeRole.Role.Token },
            { "@tlgUserId", (long)portModeRole.ClientPort.UserId },
            { "@tlgChatId", (long)portModeRole.ClientPort.ChatId },
            { "@mode", (int)portModeRole.ClientPort.Mode }
        };

        if (newStatus != DbRecordStatus.Active)
            normalParameters.Add("@date", DateTime.UtcNow);
        else
            normalParameters.Add("@date", DBNull.Value);
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
    
    public async Task HardDeleteAsync(TlgClientPortModeRole portModeRole)
    {
        const string rawQuery = "DELETE FROM tlg_client_port_mode_roles " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId " +
                                "AND interaction_mode = @mode";
        
        var normalParameters = new Dictionary<string, object>
        {
            { "@token", portModeRole.Role.Token },
            { "tlgUserId", (long)portModeRole.ClientPort.UserId },
            { "tlgChatId", (long)portModeRole.ClientPort.ChatId },
            { "@mode", (int)portModeRole.ClientPort.Mode }
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}