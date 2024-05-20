using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Persistence;
using CheckMade.Telegram.Model;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.DataMigration.Repositories;

public class MessagesMigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IEnumerable<MessageOldFormatDetailsPair>> GetMessageOldFormatDetailsPairsOrThrowAsync()
    {
        var pairBuilder = ImmutableArray.CreateBuilder<MessageOldFormatDetailsPair>();
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

    private static async Task<MessageOldFormatDetailsPair> CreateInputMessageAndDetailsInOldFormatAsync(
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
                Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None()));

        return new MessageOldFormatDetailsPair(messageWithFakeEmptyDetails, actualOldFormatDetails);
    }

    internal async Task UpdateOrThrowAsync(IEnumerable<UpdateDetails> updateDetails)
    {
        var commands = updateDetails.Select(update =>
        {
            var commandTextPrefix = "UPDATE tlgr_messages SET ";

            commandTextPrefix += string.Join(", ", update.NewValueByColumn
                .Select(d => $"{d.Key} = @{d.Key}"));
            
            commandTextPrefix = $"{commandTextPrefix} " +
                                $"WHERE user_id = @userId AND (details ->> 'TelegramDate')::timestamp = @dateTime";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            foreach(var kv in update.NewValueByColumn)
            {
                if(kv.Key == "details")
                    command.Parameters.Add(new NpgsqlParameter($"@{kv.Key}", NpgsqlDbType.Jsonb)
                    {
                        Value = kv.Value
                    });
                else
                    command.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
            }
            
            command.Parameters.AddWithValue("@userId", update.UserId);
            command.Parameters.AddWithValue("@dateTime", update.TelegramDate);

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