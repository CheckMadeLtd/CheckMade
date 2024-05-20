using System.Collections.Immutable;
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
                var telegramDate = pair.OldFormatDetailsJson.Value<string>("TelegramDate");

                if (pair.ModelMessage.ChatId == 0)
                {
                    updateDetailsBuilder.Add(new UpdateDetails(
                        pair.ModelMessage.UserId, telegramDate!,
                        new Dictionary<string, string> { { "chat_id", "1" } }));
                }

                updateDetailsBuilder.Add(new UpdateDetails(
                    pair.ModelMessage.UserId, telegramDate!,
                    new Dictionary<string, string>
                    {
                        {
                            "details",
                            JsonHelper.SerializeToJson(new MessageDetails(
                                DateTime.Parse(telegramDate!),
                                pair.OldFormatDetailsJson.Value<string>("Text")!,
                                pair.OldFormatDetailsJson.Value<string>("AttachmentUrl")
                                ?? pair.OldFormatDetailsJson.Value<string>("AttachmentExternalUrl")!,
                                Enum.Parse<AttachmentType>(
                                    pair.OldFormatDetailsJson.Value<string>("AttachmentType")!)
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