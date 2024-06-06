namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Attempt<int>> MigrateAsync(string env)
    {
        return (await 
                (from historicPairs 
                    in Attempt<IEnumerable<OldFormatDetailsPair>>.RunAsync(
                        migRepo.GetMessageOldFormatDetailsPairsOrThrowAsync)
                from updateDetails 
                    in Attempt<IEnumerable<DetailsUpdate>>.RunAsync(() => 
                        GenerateMigrationUpdatesAsync(historicPairs))
                from unit 
                    in Attempt<Unit>.RunAsync(() => 
                        MigrateHistoricMessagesAsync(updateDetails))
                select updateDetails.Count()))
            .Match(
                Attempt<int>.Succeed, 
                ex => ex);
    }

    protected abstract Task<IEnumerable<DetailsUpdate>> GenerateMigrationUpdatesAsync(
        IEnumerable<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Unit> MigrateHistoricMessagesAsync(IEnumerable<DetailsUpdate> updates)
    {
        await migRepo.UpdateOrThrowAsync(updates);

        return Unit.Value;
    }
}
