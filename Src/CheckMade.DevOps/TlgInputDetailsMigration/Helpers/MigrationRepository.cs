using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Persistence;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

public sealed class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IReadOnlyCollection<OldFormatDetailsPair>> GetMessageOldFormatDetailsPairsAsync()
    {
        var pairBuilder = ImmutableArray.CreateBuilder<OldFormatDetailsPair>();
        var command = new NpgsqlCommand("SELECT * FROM tlg_inputs");
        
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
            DateTimeOffset.MinValue, 
            0,
            new TlgAgent(tlgUserId, tlgChatId, InteractionMode.Operations),
            TlgInputType.TextMessage,
            Option<IRoleInfo>.None(), 
            Option<ILiveEventInfo>.None(), 
            Option<ResultantWorkflowInfo>.None(), 
            Option<Guid>.None(), 
            new TlgInputDetails(
                Option<string>.None(),
                Option<Uri>.None(),
                Option<Uri>.None(), 
                Option<TlgAttachmentType>.None(),
                Option<Geo>.None(), 
                Option<int>.None(),
                Option<DomainTerm>.None(), 
                Option<long>.None()));

        return new OldFormatDetailsPair(messageWithFakeEmptyDetails, actualOldFormatDetails);
    }

    internal async Task UpdateAsync(IReadOnlyCollection<DetailsUpdate> detailsUpdates)
    {
        var commands = detailsUpdates.Select(detailUpdate =>
        {
            const string commandTextPrefix = "UPDATE tlg_inputs SET details = @tlgDetails " +
                                             "WHERE user_id = @tlgUserId " +
                                             "AND (details ->> 'TlgDate')::timestamp = @tlgDateTime";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            command.Parameters.AddWithValue("@tlgUserId", detailUpdate.UserId);
            command.Parameters.AddWithValue("@tlgDateTime", detailUpdate.TlgDate);
            
            command.Parameters.Add(new NpgsqlParameter($"@tlgDetails", NpgsqlDbType.Jsonb)
            {
                Value = detailUpdate.NewDetails
            });
            
            return command;
        }).ToImmutableReadOnlyCollection();

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