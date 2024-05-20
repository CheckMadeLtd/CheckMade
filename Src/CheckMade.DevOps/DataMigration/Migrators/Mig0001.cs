using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.DevOps.DataMigration.Migrators;

internal class Mig0001(IMessageRepository messageRepo) : IDataMigrator
{
    /* Overview
     * - Only table at this point: tlgr_messages
     * - Prepares for SQL Mig 004
     * - chat_id -> NOT NULL 
     * - update old 'details' to be compatible with current MessageDetails schema
     */
    
    public async Task<Attempt<int>> MigrateAsync(string env)
    {
        return ((Attempt<int>) await 
            from allMessages in Attempt<IEnumerable<InputMessage>>
                .RunAsync(messageRepo.GetAllOrThrowAsync)
            from migratedMessages in SafelyMigrateMessagesAsync(allMessages)
            select SafelyCountUpdatedRecordsAsync(migratedMessages))
            .Match(
                Attempt<int>.Succeed, 
                ex => Attempt<int>.Fail(new DataAccessException(
                    $"Data migration failed with: {ex.Message}.", ex)));
    }

    private Attempt<IEnumerable<InputMessage>> SafelyMigrateMessagesAsync(IEnumerable<InputMessage> allMessages)
    {
        foreach (var message in allMessages)
        {
            if (message.ChatId == 0)
            {
                
            }
        }
    }

    private Attempt<int> SafelyCountUpdatedRecordsAsync(IEnumerable<InputMessage> migratedMessages)
    {
        return Attempt<int>.Succeed(1); // fake return because as of SqlMig004 there is no last_migration field yet
    }
}