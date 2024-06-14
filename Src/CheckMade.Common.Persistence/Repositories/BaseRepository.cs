using System.Collections.Immutable;
using System.Data.Common;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories;

public abstract class BaseRepository(IDbExecutionHelper dbHelper)
{
    protected static NpgsqlCommand GenerateCommand(string query, Dictionary<string, object> parameters)
    {
        var command = new NpgsqlCommand(query);

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
        }

        return command;
    }

    protected async Task ExecuteTransactionAsync(IEnumerable<NpgsqlCommand> commands)
    {
        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            foreach (var cmd in commands)
            {
                cmd.Connection = db;
                cmd.Transaction = transaction;
                await cmd.ExecuteNonQueryAsync();
            }
        });
    }

    protected async Task<IEnumerable<TModel>> ExecuteReaderAsync<TModel>(
        NpgsqlCommand command, Func<DbDataReader, TModel> readData)
    {
        var builder = ImmutableList.CreateBuilder<TModel>();

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    builder.Add(readData(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
}