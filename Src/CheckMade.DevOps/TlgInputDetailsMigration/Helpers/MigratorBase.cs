namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Attempt<int>> MigrateAsync(string env)
    {
        return (await 
                (from historicPairs 
                    in Attempt<IReadOnlyCollection<OldFormatDetailsPair>>.RunAsync(
                        migRepo.GetMessageOldFormatDetailsPairsAsync)
                from detailsUpdate 
                    in Attempt<IReadOnlyCollection<DetailsUpdate>>.RunAsync(() => 
                        GenerateMigrationUpdatesAsync(historicPairs))
                from unit 
                    in Attempt<Unit>.RunAsync(() => 
                        MigrateHistoricMessagesAsync(detailsUpdate))
                select detailsUpdate.Count()))
            .Match(
                Attempt<int>.Succeed, 
                ex => ex);
    }

    protected abstract Task<IReadOnlyCollection<DetailsUpdate>> GenerateMigrationUpdatesAsync(
        IReadOnlyCollection<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Unit> MigrateHistoricMessagesAsync(IReadOnlyCollection<DetailsUpdate> detailsUpdates)
    {
        await migRepo.UpdateAsync(detailsUpdates);

        return Unit.Value;
    }
}
