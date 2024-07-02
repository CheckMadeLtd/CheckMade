using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class UsersRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), IUsersRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<User>> _cache = Option<IReadOnlyCollection<User>>.None();

    public async Task<IReadOnlyCollection<User>> GetAllAsync()
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
                                            
                                            r.token AS role_token, 
                                            r.role_type AS role_type, 
                                            r.status AS role_status, 
                                            
                                            usr.id AS user_id, 
                                            usr.mobile AS user_mobile, 
                                            usr.first_name AS user_first_name, 
                                            usr.middle_name AS user_middle_name, 
                                            usr.last_name AS user_last_name, 
                                            usr.email AS user_email, 
                                            usr.language_setting AS user_language, 
                                            usr.status AS user_status 
                                            
                                            FROM users usr 
                                            LEFT JOIN roles r on r.user_id = usr.id 
                                            
                                            ORDER BY usr.id, r.id
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

                    var (getKey,
                        initializeModel,
                        accumulateData,
                        finalizeModel) = GetUserReader();

                    var users =
                        await ExecuteReaderOneToManyAsync(
                            command, 
                            getKey,
                            initializeModel,
                            accumulateData,
                            finalizeModel);
                    
                    _cache = Option<IReadOnlyCollection<User>>.Some(users);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
        
        return _cache.GetValueOrThrow();
    }

    public async Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage)
    {
        const string rawQuery = """
                                UPDATE users 
                                
                                SET language_setting = @newLanguage
                                
                                WHERE mobile = @mobileNumber 
                                AND status = @status
                                """;

        var normalParameters = new Dictionary<string, object>
        {
            { "@newLanguage", (int)newLanguage },
            { "@mobileNumber", user.Mobile.ToString() },
            { "@status", (int)user.Status }
        };

        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new [] { command });
        EmptyCache();
    }

    private void EmptyCache() => _cache = Option<IReadOnlyCollection<User>>.None();
}