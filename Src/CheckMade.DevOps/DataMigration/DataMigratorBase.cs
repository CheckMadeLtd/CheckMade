using CheckMade.Common.FpExt;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils;
using CheckMade.DevOps.DataMigration.Repositories;

namespace CheckMade.DevOps.DataMigration;

internal abstract class DataMigratorBase(MessagesMigrationRepository migRepo)
{
    internal async Task<Attempt<int>> MigrateAsync(string env)
    {
        return ((Attempt<int>) await (
                from historicPairs in Attempt<IEnumerable<MessageOldFormatDetailsPair>>
                    .RunAsync(migRepo.GetMessageOldFormatDetailsPairsOrThrowAsync)
                from updateDetails in SafelyGenerateMigrationUpdatesAsync(historicPairs)
                from migratedMessages in SafelyMigrateHistoricMessages(updateDetails)
                select updateDetails.Count())
            ).Match(
                Attempt<int>.Succeed, 
                ex => Attempt<int>.Fail(new DataAccessException(
                    $"Data migration failed with: {ex.Message}.", ex)));
    }

    protected abstract Attempt<IEnumerable<UpdateDetails>> SafelyGenerateMigrationUpdatesAsync(
        IEnumerable<MessageOldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Attempt<Unit>> SafelyMigrateHistoricMessages(IEnumerable<UpdateDetails> updates)
    {
        try
        {
            await migRepo.UpdateOrThrowAsync(updates);
        }
        catch (Exception ex)
        {
            return Attempt<Unit>.Fail(new DataMigrationException(
                $"Exception while performing data migration updates: {ex.Message}", ex));
        }

        return Attempt<Unit>.Succeed(Unit.Value);
    }
}
