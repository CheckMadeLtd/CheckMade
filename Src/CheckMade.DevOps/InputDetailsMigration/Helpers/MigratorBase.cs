using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal abstract class MigratorBase(MigrationRepository migRepo)
{
    internal async Task<Result<int>> MigrateAsync()
    {
        return (await 
                (from allOldFormatDetails 
                        in Result<IReadOnlyCollection<OldFormatDetails>>.RunAsync(
                            migRepo.GetOldFormatDetailsAsync)
                    from allNewFormatDetails 
                        in Result<IReadOnlyCollection<NewFormatDetails>>.Run(() => 
                            ConvertOldToNewAsync(allOldFormatDetails))
                    from unit 
                        in Result<Unit>.RunAsync(() => 
                            MigrateHistoricInputsAsync(allNewFormatDetails))
                    select allNewFormatDetails.Count))
            .Match(
                Result<int>.Succeed, 
                static failure => failure);
    }

    protected abstract IReadOnlyCollection<NewFormatDetails> ConvertOldToNewAsync(
        IReadOnlyCollection<OldFormatDetails> allOldFormatDetails);
    
    private async Task<Unit> MigrateHistoricInputsAsync(IReadOnlyCollection<NewFormatDetails> allNewFormatDetails)
    {
        await migRepo.UpdateAsync(allNewFormatDetails);

        return Unit.Value;
    }
}
