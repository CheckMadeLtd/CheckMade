using CheckMade.Common.Utils;
using CheckMade.Common.Utils.MonadicWrappers;
using CheckMade.Telegram.Interfaces;

namespace CheckMade.DevOps.DataMigration.Migrators;

internal class Mig0001(IMessageRepository messageRepo) : IDataMigrator
{
    /* Overview
     * - Only table at this point: tlgr_messages
     * - Prepares for SQL Mig 004
     * - chat_id -> NOT NULL 
     * - update old 'details' to be compatible with current MessageDetails schema
     */
    
    public async Task<Result<int>> MigrateAsync(string env)
    {
        var allMessages = await messageRepo.GetAllAsync();
        
        // Do the processing / mapping etc. and count how many records were updated.

        var numberOfRecordsUpdated = 3;
        
        return numberOfRecordsUpdated;
    }
}