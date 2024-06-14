using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class RoleRepository(IDbExecutionHelper dbHelper) : BaseRepository(dbHelper), IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        var command = GenerateCommand("SELECT * FROM roles", Option<Dictionary<string, object>>.None());

        return await ExecuteReaderAsync(command, reader =>
            new Role(
                reader.GetString(reader.GetOrdinal("token")),
                (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("status"))));
    }
}