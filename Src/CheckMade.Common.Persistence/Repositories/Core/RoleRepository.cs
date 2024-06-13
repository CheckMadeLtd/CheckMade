using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class RoleRepository(IDbExecutionHelper dbHelper) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync() 
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        var command = new NpgsqlCommand("SELECT * FROM roles");

        await dbHelper.ExecuteAsync(async (db, transaction) => 
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var role = new Role(
                        reader.GetString(reader.GetOrdinal("token")),
                        (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
                        (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("status")));

                    builder.Add(role);
                }
            }
        });

        return builder.ToImmutable();
    }
}
