using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Services.Persistence;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

public sealed class MigrationRepository(IDbExecutionHelper dbHelper)
{
    internal async Task<IReadOnlyCollection<OldFormatDetails>> GetOldFormatDetailsAsync()
    {
        var oldDetailsBuilder = ImmutableArray.CreateBuilder<OldFormatDetails>();
        var command = new NpgsqlCommand("SELECT * FROM inputs");
        
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
            },
            [command]);

        return oldDetailsBuilder.ToImmutable();

        static async Task<OldFormatDetails> CreateOldFormatDetailsInstanceAsync(DbDataReader reader)
        {
            var id = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("id"));
        
            var actualOldFormatDetails = JObject.Parse(
                await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details")));
        
            return new OldFormatDetails(id, actualOldFormatDetails);
        }
    }

    internal async Task UpdateAsync(IReadOnlyCollection<NewFormatDetails> allNewFormatDetails)
    {
        var commands = allNewFormatDetails.Select(static newDetails =>
        {
            const string commandTextPrefix = "UPDATE inputs SET details = @inputDetails " +
                                             "WHERE id = @id";

            var command = new NpgsqlCommand(commandTextPrefix);
            
            command.Parameters.AddWithValue("@id", newDetails.Id);
            
            command.Parameters.Add(new NpgsqlParameter($"@inputDetails", NpgsqlDbType.Jsonb)
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
            },
            commands);
    }
}