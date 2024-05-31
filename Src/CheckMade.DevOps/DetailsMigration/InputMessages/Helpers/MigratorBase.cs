using CheckMade.Common.LangExt;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Attempt<int>> MigrateAsync(string env)
    {
        return ((Attempt<int>) await (
                from historicPairs in Attempt<IEnumerable<OldFormatDetailsPair>>
                    .RunAsync(migRepo.GetMessageOldFormatDetailsPairsOrThrowAsync)
                from updateDetails in GenerateMigrationUpdatesAsync(historicPairs)
                from unit in MigrateHistoricMessages(updateDetails)
                select updateDetails.Count())
            ).Match(
                Attempt<int>.Succeed, 
                failure => Attempt<int>.Fail(
                    failure with // preserves any contained Exception and prefixes any contained Error UiString
                {
                    Error = UiConcatenate(
                        Ui("Data migration failed."),
                        failure.Error)
                }));
    }

    protected abstract Attempt<IEnumerable<DetailsUpdate>> GenerateMigrationUpdatesAsync(
        IEnumerable<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Attempt<Unit>> MigrateHistoricMessages(IEnumerable<DetailsUpdate> updates)
    {
        try
        {
            await migRepo.UpdateOrThrowAsync(updates);
        }
        catch (Exception ex)
        {
            return new Failure(new DataMigrationException(
                $"Exception while performing data migration updates: {ex.Message}.", ex));
        }

        return Unit.Value;
    }
}
