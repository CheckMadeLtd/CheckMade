using System.Collections.Immutable;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Services.Persistence.JsonHelpers;
using CheckMade.Bot.Workflows;
using CheckMade.DevOps.InputDetailsMigration.Helpers;

namespace CheckMade.DevOps.InputDetailsMigration.Migrators;

internal class Mig0003(MigrationRepository migRepo) : MigratorBase(migRepo)
{
    /*
     * Overview:
     * - Necessary for v1.2.2
     * - Removal of AttachmentTlgUri in InputDetails
     */
    
    protected override IReadOnlyCollection<NewFormatDetails> ConvertOldToNewAsync(
        IReadOnlyCollection<OldFormatDetails> allOldFormatDetails)
    {
        var newFormatDetailsBuilder = ImmutableArray.CreateBuilder<NewFormatDetails>();
        var glossary = new DomainGlossary();

        foreach (var oldDetails in allOldFormatDetails)
        {
            var detailsWithoutTlgUri = JsonHelper.DeserializeFromJson<InputDetails>(
                oldDetails.OldFormatDetailsJson.ToString(), glossary, true);

            var newDetails = new NewFormatDetails(
                oldDetails.Identifier,
                JsonHelper.SerializeToJson(detailsWithoutTlgUri, glossary));
            
            newFormatDetailsBuilder.Add(newDetails);
        }

        return newFormatDetailsBuilder.ToImmutable();
    }
}