using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core.Actors;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class RolesRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), IRolesRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<Role>> _cache = Option<IReadOnlyCollection<Role>>.None();

    public async Task<Role?> GetAsync(IRoleInfo role) =>
        (await GetAllAsync())
        .FirstOrDefault(r => r.Equals(role));

    public async Task<IReadOnlyCollection<Role>> GetAllAsync()
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
                                            r.status AS role_status 

                                            FROM roles r 
                                            INNER JOIN users usr on r.user_id = usr.id 
                                            INNER JOIN live_events lve on r.live_event_id = lve.id 
                                            
                                            ORDER BY r.id;
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());

                    _cache = Option<IReadOnlyCollection<Role>>.Some(
                        new List<Role>(await ExecuteReaderOneToOneAsync(command, ModelReaders.ReadRole))
                            .ToImmutableReadOnlyCollection());
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
        
        return _cache.GetValueOrThrow();
    }
}