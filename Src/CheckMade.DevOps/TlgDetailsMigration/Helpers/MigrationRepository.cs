using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Model.UserInteraction;
using CheckMade.Common.Persistence;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.TlgDetailsMigration.Helpers;

public class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IEnumerable<OldFormatDetailsPair>> GetMessageOldFormatDetailsPairsAsync()
    {
        var pairBuilder = ImmutableArray.CreateBuilder<OldFormatDetailsPair>();
        var command = new NpgsqlCommand("SELECT * FROM tlgr_updates");
        
        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    pairBuilder.Add(await CreateTlgInputAndDetailsInOldFormatAsync(reader));
                }
            }
        });

        return pairBuilder.ToImmutable();
    }

    private static async Task<OldFormatDetailsPair> CreateTlgInputAndDetailsInOldFormatAsync(
        DbDataReader reader)
    {
        TlgUserId tlgUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        TlgChatId tlgChatId = await reader.GetFieldValueAsync<long?>(reader.GetOrdinal("chat_id"))
            ?? 0;
        var actualOldFormatDetails = JObject.Parse(
            await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details")));
        
        var messageWithFakeEmptyDetails = new TlgInput(
            tlgUserId,
            tlgChatId,
            InteractionMode.Operations,
            TlgInputType.TextMessage,
            new TlgInputDetails(DateTime.MinValue,
                0,
                Option<string>.None(),
                Option<Uri>.None(),
                Option<Uri>.None(), 
                Option<TlgAttachmentType>.None(),
                Option<Geo>.None(), 
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        return new OldFormatDetailsPair(messageWithFakeEmptyDetails, actualOldFormatDetails);
    }

    internal async Task UpdateAsync(IEnumerable<DetailsUpdate> detailsUpdates)
    {
        var commands = detailsUpdates.Select(detailUpdate =>
        {
            const string commandTextPrefix = "UPDATE tlgr_updates SET details = @tlgDetails " +
                                             "WHERE user_id = @tlgUserId " +
                                             "AND (details ->> 'TlgDate')::timestamp = @tlgDateTime";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            command.Parameters.AddWithValue("@tlgUserId", detailUpdate.UserId);
            command.Parameters.AddWithValue("@tlgDateTime", detailUpdate.TelegramDate);
            
            command.Parameters.Add(new NpgsqlParameter($"@tlgDetails", NpgsqlDbType.Jsonb)
            {
                Value = detailUpdate.NewDetails
            });
            
            return command;
        }).ToImmutableArray();

        await dbHelper.ExecuteAsync(async (db, transaction) =>
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