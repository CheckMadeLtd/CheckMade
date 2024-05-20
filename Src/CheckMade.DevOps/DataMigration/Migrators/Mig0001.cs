using System.Collections.Immutable;
using System.Globalization;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Persistence;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Common.Utils;
using CheckMade.DevOps.DataMigration.Repositories;
using CheckMade.Telegram.Model;

namespace CheckMade.DevOps.DataMigration.Migrators;

internal class Mig0001(MessagesMigrationRepository migRepo) : DataMigratorBase(migRepo)
{
    /* Overview
     * - Only table at this point: tlgr_messages
     * - Prepares for SQL Mig 004
     * - chat_id -> NOT NULL 
     * - update old 'details' to be compatible with current MessageDetails schema
     */
    
    protected override Attempt<IEnumerable<UpdateDetails>> SafelyGenerateMigrationUpdatesAsync(
        IEnumerable<MessageOldFormatDetailsPair> allHistoricMessageDetailPairs)
    {
        var updateDetailsBuilder = ImmutableArray.CreateBuilder<UpdateDetails>();

        try
        {
            foreach (var pair in allHistoricMessageDetailPairs)
            {
                DateTime.TryParseExact(
                    pair.OldFormatDetailsJson.Value<string>("TelegramDate"), 
                    "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, 
                    out var telegramDate);
                
                var updateDetailsForCurrentPair = new UpdateDetails(
                    pair.ModelMessage.UserId, telegramDate,
                    new Dictionary<string, object>());
                
                if (pair.ModelMessage.ChatId == 0)
                {
                    updateDetailsForCurrentPair.NewValueByColumn.Add("chat_id", 1);
                }

                // Interpret / convert the details in the old format...
                

                var attachmentUrlRaw = pair.OldFormatDetailsJson.Value<string>("AttachmentUrl")
                                       ?? pair.OldFormatDetailsJson.Value<string>("AttachmentExternalUrl");
                var attachmentUrl = !string.IsNullOrWhiteSpace(attachmentUrlRaw)
                    ? Option<string>.Some(attachmentUrlRaw)
                    : Option<string>.None();
                
                var attachmentTypeString = pair.OldFormatDetailsJson.Value<string>("AttachmentType");
                var attachmentType = !string.IsNullOrWhiteSpace(attachmentTypeString) 
                    ? Enum.Parse<AttachmentType>(attachmentTypeString) 
                    : Option<AttachmentType>.None();
                
                // Now use the interpreted values to create a new, current-format MessageDetails
                updateDetailsForCurrentPair.NewValueByColumn.Add(
                    "details",
                    JsonHelper.SerializeToJson(new MessageDetails(
                        telegramDate,
                        pair.OldFormatDetailsJson.Value<string>("Text")!,
                        attachmentUrl,
                        attachmentType))
                    );
                
                updateDetailsBuilder.Add(updateDetailsForCurrentPair);
            }
        }
        catch (Exception ex)
        {
            return Attempt<IEnumerable<UpdateDetails>>
                .Fail(new DataMigrationException(
                    $"Exception while generating updates for data migration: {ex.Message}", ex));
        }

        return Attempt<IEnumerable<UpdateDetails>>
            .Succeed(updateDetailsBuilder.ToImmutable());
    }
}