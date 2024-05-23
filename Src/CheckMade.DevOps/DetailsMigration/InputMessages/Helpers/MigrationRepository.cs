using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Persistence;
using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

public class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IEnumerable<OldFormatDetailsPair>> GetMessageOldFormatDetailsPairsOrThrowAsync()
    {
        var pairBuilder = ImmutableArray.CreateBuilder<OldFormatDetailsPair>();
        var command = new NpgsqlCommand("SELECT * FROM tlgr_messages");
        
        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    pairBuilder.Add(await CreateInputMessageAndDetailsInOldFormatAsync(reader));
                }
            }
        });

        return pairBuilder.ToImmutable();
    }

    private static async Task<OldFormatDetailsPair> CreateInputMessageAndDetailsInOldFormatAsync(
        DbDataReader reader)
    {
        var telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        var telegramChatId = await reader.GetFieldValueAsync<long?>(reader.GetOrdinal("chat_id"))
            ?? 0;
        var actualOldFormatDetails = JObject.Parse(
            await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details")));
        
        var messageWithFakeEmptyDetails = new InputMessage(
            telegramUserId,
            telegramChatId,
            new MessageDetails(DateTime.MinValue,
                BotType.Submissions,
                Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None()));

        return new OldFormatDetailsPair(messageWithFakeEmptyDetails, actualOldFormatDetails);
    }

    internal async Task UpdateOrThrowAsync(IEnumerable<DetailsUpdate> updates)
    {
        var commands = updates.Select(update =>
        {
            const string commandTextPrefix = "UPDATE tlgr_messages SET details = @details " +
                                             "WHERE user_id = @userId " +
                                             "AND (details ->> 'TelegramDate')::timestamp = @dateTime";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            command.Parameters.AddWithValue("@userId", update.UserId);
            command.Parameters.AddWithValue("@dateTime", update.TelegramDate);
            
            command.Parameters.Add(new NpgsqlParameter($"@details", NpgsqlDbType.Jsonb)
            {
                Value = update.NewDetails
            });
            
            return command;
        }).ToImmutableArray();

        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            foreach (var command in commands)
            {
                command.Connection = db;
                command.Transaction = transaction;
                await command.ExecuteNonQueryAsync();
            }
        });
    }
}