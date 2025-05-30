using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Persistence;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

public sealed class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IReadOnlyCollection<OldFormatDetails>> GetOldFormatDetailsAsync()
    {
        var oldDetailsBuilder = ImmutableArray.CreateBuilder<OldFormatDetails>();
        var command = new NpgsqlCommand("SELECT * FROM tlg_inputs");
        
        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    oldDetailsBuilder.Add(await CreateOldFormatDetailsInstanceAsync(reader));
                }
            }
        });

        return oldDetailsBuilder.ToImmutable();

        static async Task<OldFormatDetails> CreateOldFormatDetailsInstanceAsync(DbDataReader reader)
        {
            var identifier = await reader.Fork(
                static async r => await r.GetFieldValueAsync<long>(r.GetOrdinal("user_id")),
                static async r => await r.GetFieldValueAsync<DateTimeOffset>(r.GetOrdinal("date")),
                static async (historicUserId, historicTlgDate) => 
                    new HistoricInputIdentifier(new TlgUserId(await historicUserId), await historicTlgDate)
            );
        
            var actualOldFormatDetails = JObject.Parse(
                await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details")));
        
            return new OldFormatDetails(identifier, actualOldFormatDetails);
        }
    }

    internal async Task UpdateAsync(IReadOnlyCollection<NewFormatDetails> allNewFormatDetails)
    {
        var commands = allNewFormatDetails.Select(static newDetails =>
        {
            const string commandTextPrefix = "UPDATE tlg_inputs SET details = @tlgDetails " +
                                             "WHERE user_id = @tlgUserId " +
                                             "AND date = @tlgDateTime";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            command.Parameters.AddWithValue("@tlgUserId", newDetails.Identifier.HistoricUserId.Id);
            command.Parameters.AddWithValue("@tlgDateTime", newDetails.Identifier.HistoricTlgDate);
            
            command.Parameters.Add(new NpgsqlParameter($"@tlgDetails", NpgsqlDbType.Jsonb)
            {
                Value = newDetails.NewDetails
            });
            
            return command;
        }).ToArray();

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