using System.Collections.Immutable;
using System.Globalization;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Common.Utils;
using CheckMade.DevOps.DetailsMigration.Repositories.Messages;
using CheckMade.Telegram.Model;

namespace CheckMade.DevOps.DetailsMigration.Migrators;

internal class Mig0001(MessagesMigrationRepository migRepo) : DetailsMigratorBase(migRepo)
{
    /* Overview
     * - Only table at this point: tlgr_messages
     * - Prepares for SQL Mig 004
     * - chat_id -> NOT NULL 
     * - update old 'details' to be compatible with current MessageDetails schema
     */
    
    protected override Attempt<IEnumerable<MessageDetailsUpdate>> SafelyGenerateMigrationUpdatesAsync(
        IEnumerable<MessageOldFormatDetailsPair> allHistoricMessageDetailPairs)
    {
        var updatesBuilder = ImmutableArray.CreateBuilder<MessageDetailsUpdate>();

        try
        {
            foreach (var pair in allHistoricMessageDetailPairs)
            {
                var update = Option<MessageDetailsUpdate>.None();
                
                DateTime.TryParseExact(
                    pair.OldFormatDetailsJson.Value<string>("TelegramDate"), 
                    "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, 
                    out var telegramDate);
                
                var deserializationOfDetailsAttempt = Attempt<MessageDetails?>.Run(() => 
                    JsonHelper.DeserializeFromJsonStrict<MessageDetails>(pair.OldFormatDetailsJson.ToString()));

                if (deserializationOfDetailsAttempt.IsFailure)
                {
                    // Interpret the details from the old format JObject, so they can be used for the new format...

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
                
                    update = new MessageDetailsUpdate(
                        pair.ModelMessage.UserId, telegramDate, 
                        JsonHelper.SerializeToJson(new MessageDetails(
                            telegramDate,
                            pair.OldFormatDetailsJson.Value<string>("Text")!,
                            attachmentUrl,
                            attachmentType)
                        ));
                }
                
                if (update.IsSome)
                    updatesBuilder.Add(update.GetValueOrDefault());
            }
        }
        catch (Exception ex)
        {
            return Attempt<IEnumerable<MessageDetailsUpdate>>
                .Fail(new DataMigrationException(
                    $"Exception while generating updates for data migration: {ex.Message}", ex));
        }

        return Attempt<IEnumerable<MessageDetailsUpdate>>
            .Succeed(updatesBuilder.ToImmutable());
    }
}