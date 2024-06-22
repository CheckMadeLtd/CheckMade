using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class UsersRepository(IDbExecutionHelper dbHelper, ILogger<BaseRepository> logger) 
    : BaseRepository(dbHelper, logger), IUsersRepository
{
    public async Task UpdateLanguageSettingAsync(User user, LanguageCode newLanguage)
    {
        const string rawQuery = "UPDATE users " +
                                "SET language_setting = @newLanguage " +
                                "WHERE mobile = @mobileNumber";

        var normalParameters = new Dictionary<string, object>
        {
            { "@newLanguage", (int)newLanguage },
            { "@mobileNumber", user.Mobile.ToString() }
        };

        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}