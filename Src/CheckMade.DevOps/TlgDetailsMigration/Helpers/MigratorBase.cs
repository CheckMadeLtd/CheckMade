namespace CheckMade.DevOps.TlgDetailsMigration.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Attempt<int>> MigrateAsync(string env)
    {
        return (await 
                (from historicPairs 
                    in Attempt<IEnumerable<OldFormatDetailsPair>>.RunAsync(
                        migRepo.GetMessageOldFormatDetailsPairsAsync)
                from detailsUpdate 
                    in Attempt<IEnumerable<DetailsUpdate>>.RunAsync(() => 
                        GenerateMigrationUpdatesAsync(historicPairs))
                from unit 
                    in Attempt<Unit>.RunAsync(() => 
                        MigrateHistoricMessagesAsync(detailsUpdate))
                select detailsUpdate.Count()))
            .Match(
                Attempt<int>.Succeed, 
                ex => ex);
    }

    protected abstract Task<IEnumerable<DetailsUpdate>> GenerateMigrationUpdatesAsync(
        IEnumerable<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Unit> MigrateHistoricMessagesAsync(IEnumerable<DetailsUpdate> detailsUpdates)
    {
        await migRepo.UpdateAsync(detailsUpdates);

        return Unit.Value;
    }
}
