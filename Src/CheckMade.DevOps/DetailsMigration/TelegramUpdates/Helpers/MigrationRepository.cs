using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model;
using CheckMade.Common.Model.TelegramUpdates;
using CheckMade.Common.Persistence;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

public class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IEnumerable<OldFormatDetailsPair>> GetMessageOldFormatDetailsPairsOrThrowAsync()
    {
        var pairBuilder = ImmutableArray.CreateBuilder<OldFormatDetailsPair>();
        var command = new NpgsqlCommand("SELECT * FROM tlgr_updates");
        
        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    pairBuilder.Add(await CreateTelegramUpdateAndDetailsInOldFormatAsync(reader));
                }
            }
        });

        return pairBuilder.ToImmutable();
    }

    private static async Task<OldFormatDetailsPair> CreateTelegramUpdateAndDetailsInOldFormatAsync(
        DbDataReader reader)
    {
        TelegramUserId telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        TelegramChatId telegramChatId = await reader.GetFieldValueAsync<long?>(reader.GetOrdinal("chat_id"))
            ?? 0;
        var actualOldFormatDetails = JObject.Parse(
            await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details")));
        
        var messageWithFakeEmptyDetails = new TelegramUpdate(
            telegramUserId,
            telegramChatId,
            BotType.Operations,
            ModelUpdateType.TextMessage,
            new TelegramUpdateDetails(DateTime.MinValue,
                0,
                Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        return new OldFormatDetailsPair(messageWithFakeEmptyDetails, actualOldFormatDetails);
    }

    internal async Task UpdateOrThrowAsync(IEnumerable<DetailsUpdate> updates)
    {
        var commands = updates.Select(update =>
        {
            const string commandTextPrefix = "UPDATE tlgr_updates SET details = @details " +
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