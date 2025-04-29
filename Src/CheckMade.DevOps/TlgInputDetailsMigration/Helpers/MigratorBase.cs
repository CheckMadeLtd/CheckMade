using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Result<int>> MigrateAsync()
    {
        return (await 
                (from historicPairs 
                        in Result<IReadOnlyCollection<OldFormatDetailsPair>>.RunAsync(
                            migRepo.GetMessageOldFormatDetailsPairsAsync)
                    from detailsUpdate 
                        in Result<IReadOnlyCollection<DetailsUpdate>>.RunAsync(() => 
                            GenerateMigrationUpdatesAsync(historicPairs))
                    from unit 
                        in Result<Unit>.RunAsync(() => 
                            MigrateHistoricMessagesAsync(detailsUpdate))
                    select detailsUpdate.Count))
            .Match(
                Result<int>.Succeed, 
                static failure => failure);
    }

    protected abstract Task<IReadOnlyCollection<DetailsUpdate>> GenerateMigrationUpdatesAsync(
        IReadOnlyCollection<OldFormatDetailsPair> allHistoricMessageDetailPairs);
    
    private async Task<Unit> MigrateHistoricMessagesAsync(IReadOnlyCollection<DetailsUpdate> detailsUpdates)
    {
        await migRepo.UpdateAsync(detailsUpdates);

        return Unit.Value;
    }
}
