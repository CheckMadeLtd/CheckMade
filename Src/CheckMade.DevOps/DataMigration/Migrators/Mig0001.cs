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
                var telegramDateString = pair.OldFormatDetailsJson.Value<string>("TelegramDate");
                
                if (pair.ModelMessage.ChatId == 0)
                {
                    updateDetailsBuilder.Add(new UpdateDetails(
                        pair.ModelMessage.UserId, telegramDateString!,
                        new Dictionary<string, string> { { "chat_id", "1" } }));
                }

                const string format = "MM/dd/yyyy HH:mm:ss";
                DateTime.TryParseExact(
                    telegramDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var telegramDate);

                var attachmentTypeString = pair.OldFormatDetailsJson.Value<string>("AttachmentType");
                
                var attachmentType = !string.IsNullOrWhiteSpace(attachmentTypeString) 
                    ? Enum.Parse<AttachmentType>(attachmentTypeString) 
                    : Option<AttachmentType>.None();
                
                updateDetailsBuilder.Add(new UpdateDetails(
                    pair.ModelMessage.UserId, telegramDateString!,
                    new Dictionary<string, string>
                    {
                        {
                            "details",
                            JsonHelper.SerializeToJson(new MessageDetails(
                                telegramDate,
                                pair.OldFormatDetailsJson.Value<string>("Text")!,
                                pair.OldFormatDetailsJson.Value<string>("AttachmentUrl")
                                ?? pair.OldFormatDetailsJson.Value<string>("AttachmentExternalUrl")!,
                                attachmentType
                            ))
                        }
                    })
                );
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