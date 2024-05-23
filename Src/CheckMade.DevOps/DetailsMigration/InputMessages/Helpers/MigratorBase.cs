using CheckMade.Common.LangExt;
using CheckMade.Common.LangExt.MonadicWrappers;
using CheckMade.Common.Utils;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Attempt<int>> SafelyMigrateAsync(string env)
    {
        return ((Attempt<int>) await (
                from historicPairs in Attempt<IEnumerable<OldFormatDetailsPair>>
                    .RunAsync(migRepo.GetMessageOldFormatDetailsPairsOrThrowAsync)
                from updateDetails in SafelyGenerateMigrationUpdatesAsync(historicPairs)
                from unit in SafelyMigrateHistoricMessages(updateDetails)
                select updateDetails.Count())
            ).Match(
                Attempt<int>.Succeed, 
                ex => Attempt<int>.Fail(new DataAccessException(
                    $"Data migration failed with: {ex.Message}.", ex)));
    }

    protected abstract Attempt<IEnumerable<DetailsUpdate>> SafelyGenerateMigrationUpdatesAsync(
        IEnumerable<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Attempt<Unit>> SafelyMigrateHistoricMessages(IEnumerable<DetailsUpdate> updates)
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
