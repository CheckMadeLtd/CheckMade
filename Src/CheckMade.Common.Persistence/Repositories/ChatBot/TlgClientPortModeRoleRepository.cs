using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgClientPortModeRoleRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgClientPortModeRoleRepository
{
    public async Task AddAsync(TlgClientPortModeRole portModeRole)
    {
        const string rawQuery = "INSERT INTO tlg_client_port_roles (" +
                                "role_id, tlg_user_id, tlg_chat_id, activation_date, deactivation_date, status) " +
                                "VALUES ((SELECT id FROM roles WHERE token = @token), @tlgUserId, @tlgChatId, " +
                                "@activationDate, @deactivationDate, @status)";

        var normalParameters = new Dictionary<string, object>
        {
            { "@token", portModeRole.Role.Token },
            { "@tlgUserId", (long)portModeRole.ClientPort.UserId },
            { "@tlgChatId", (long)portModeRole.ClientPort.ChatId },
            { "@activationDate", portModeRole.ActivationDate },
            { "@status", (int)portModeRole.Status }
        };

        if (portModeRole.DeactivationDate.IsSome)
            normalParameters.Add("@deactivationDate", portModeRole.DeactivationDate.GetValueOrThrow());
        else
            normalParameters.Add("@deactivationDate", DBNull.Value);

        await ExecuteTransactionAsync(new List<NpgsqlCommand>
        {
            GenerateCommand(rawQuery, normalParameters)
        });
    }

    public async Task<IEnumerable<TlgClientPortModeRole>> GetAllAsync()
    {
        const string rawQuery = "SELECT r.token, r.role_type, r.status, tlr.tlg_user_id, tlr.tlg_chat_id, " +
                                "tlr.activation_date, tlr.deactivation_date, tlr.status " +
                                "FROM tlg_client_port_roles tlr " +
                                "JOIN roles r on tlr.role_id = r.id";

        var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

        return await ExecuteReaderAsync(command, reader =>
        {
            var role = new Role(
                reader.GetString(0),
                (RoleType)reader.GetInt16(1),
                (DbRecordStatus)reader.GetInt16(2));
                    
            var clientPort = new TlgClientPort(
                reader.GetInt64(3),
                reader.GetInt64(4));
                    
            var activationDate = reader.GetDateTime(5);
                    
            var deactivationDate = !reader.IsDBNull(6) 
                ? Option<DateTime>.Some(reader.GetDateTime(6)) 
                : Option<DateTime>.None();
                    
            var status = (DbRecordStatus)reader.GetInt16(7);

            return new TlgClientPortModeRole(role, clientPort, activationDate, deactivationDate, status);
        });
    }

    public async Task UpdateStatusAsync(TlgClientPortModeRole portModeRole, DbRecordStatus newStatus)
    {
        const string rawQuery = "UPDATE tlg_client_port_roles " +
                                "SET status = @status, deactivation_date = @date " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId";
    
        var normalParameters = new Dictionary<string, object>
        {
            { "@status", (int)newStatus },
            { "@token", portModeRole.Role.Token },
            { "@tlgUserId", (long)portModeRole.ClientPort.UserId },
            { "@tlgChatId", (long)portModeRole.ClientPort.ChatId },
        };

        if (newStatus != DbRecordStatus.Active)
            normalParameters.Add("@date", DateTime.Now);
        else
            normalParameters.Add("@date", DBNull.Value);
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
    
    public async Task HardDeleteAsync(TlgClientPortModeRole portModeRole)
    {
        const string rawQuery = "DELETE FROM tlg_client_port_roles " +
                                "WHERE role_id = (SELECT id FROM roles WHERE token = @token) " +
                                "AND tlg_user_id = @tlgUserId " +
                                "AND tlg_chat_id = @tlgChatId";
        
        var normalParameters = new Dictionary<string, object>
        {
            { "@token", portModeRole.Role.Token },
            { "tlgUserId", (long)portModeRole.ClientPort.UserId },
            { "tlgChatId", (long)portModeRole.ClientPort.ChatId },
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}