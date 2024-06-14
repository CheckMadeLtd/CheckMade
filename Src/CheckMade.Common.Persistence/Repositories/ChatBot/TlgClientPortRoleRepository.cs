using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgClientPortRoleRepository(IDbExecutionHelper dbHelper) : ITlgClientPortRoleRepository
{
    public async Task AddAsync(TlgClientPortRole portRole)
    {
        var command = new NpgsqlCommand(
            "INSERT INTO tlg_client_port_roles (role_id, tlg_user_id, tlg_chat_id, activation_date, " +
            "deactivation_date, status) " +
            "VALUES ((SELECT id FROM roles WHERE token = @token), @tlgUserId, @tlgChatId, @activationDate, " +
            "@deactivationDate, @status)");

        command.Parameters.AddWithValue("@token", portRole.Role.Token);
        command.Parameters.AddWithValue("@tlgUserId", (long) portRole.ClientPort.UserId);
        command.Parameters.AddWithValue("@tlgChatId", (long) portRole.ClientPort.ChatId);
        command.Parameters.AddWithValue("@activationDate", portRole.ActivationDate);

        if (portRole.DeactivationDate.IsSome)
            command.Parameters.AddWithValue("@deactivationDate", portRole.DeactivationDate.GetValueOrThrow());
        else
            command.Parameters.AddWithValue("@deactivationDate", DBNull.Value);

        command.Parameters.AddWithValue("@status", (int)portRole.Status);

        await dbHelper.ExecuteAsync(async (db, transaction) => 
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }

    public async Task<IEnumerable<TlgClientPortRole>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<TlgClientPortRole>();
        
        var command = new NpgsqlCommand(
            "SELECT r.token, r.role_type, r.status, tlr.tlg_user_id, tlr.tlg_chat_id, tlr.activation_date, " +
            "tlr.deactivation_date, tlr.status " +
            "FROM tlg_client_port_roles tlr " +
            "JOIN roles r on tlr.role_id = r.id");

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
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
                    builder.Add(new TlgClientPortRole(role, clientPort, activationDate, deactivationDate, status));
                }
            }
        });
        
        return builder.ToImmutable();
    }
}