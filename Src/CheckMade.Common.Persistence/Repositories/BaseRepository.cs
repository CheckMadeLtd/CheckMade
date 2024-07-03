using System.Collections.Immutable;
using System.Data.Common;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories;

public abstract class BaseRepository(IDbExecutionHelper dbHelper)
{
    protected static NpgsqlCommand GenerateCommand(string query, Option<Dictionary<string, object>> parameters)
    {
        var command = new NpgsqlCommand(query);

        if (parameters.IsSome)
        {
            foreach (var parameter in parameters.GetValueOrThrow())
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        return command;
    }

    protected async Task ExecuteTransactionAsync(IReadOnlyCollection<NpgsqlCommand> commands)
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

    protected async Task<IReadOnlyCollection<TModel>> ExecuteReaderOneToOneAsync<TModel>(
        NpgsqlCommand command, Func<DbDataReader, TModel> readData)
    {
        var builder = ImmutableList.CreateBuilder<TModel>();
        
        await ExecuteReaderCoreAsync(command, async reader =>
        {
            while (await reader.ReadAsync())
            {
                builder.Add(readData(reader));
            }
        });
        return builder.ToImmutable();
    }

    protected async Task<IReadOnlyCollection<TModel>> ExecuteReaderOneToManyAsync<TModel, TKey>(
        NpgsqlCommand command, 
        Func<DbDataReader, TKey> getKey,
        Func<DbDataReader, TModel> initializeModel,
        Action<TModel, DbDataReader> accumulateData,
        Func<TModel, TModel> finalizeModel)
    {
        var builder = ImmutableList.CreateBuilder<TModel>();

        await ExecuteReaderCoreAsync(command, async reader =>
        {
            TModel? currentModel = default;
            TKey? currentKey = default;

            while (await reader.ReadAsync())
            {
                var key = getKey(reader);

                if (!Equals(key, currentKey))
                {
                    if (currentModel != null)
                    {
                        builder.Add(finalizeModel(currentModel));
                    }
                    currentModel = initializeModel(reader);
                    currentKey = key;
                }

                accumulateData(currentModel!, reader);
            }

            if (currentModel != null)
            {
                builder.Add(finalizeModel(currentModel));
            }
        });

        return builder.ToImmutable();
    }

    private async Task ExecuteReaderCoreAsync(
        NpgsqlCommand command,
        Func<DbDataReader, Task> processReader)
    {
        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;

            await using var reader = await command.ExecuteReaderAsync();
            await processReader(reader);
        });
    }
}