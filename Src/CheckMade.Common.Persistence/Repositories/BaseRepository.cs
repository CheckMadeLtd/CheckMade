using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Utils.FpExtensions.Monads;
using Npgsql;

namespace CheckMade.Common.Persistence.Repositories;

public abstract class BaseRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary)
{
    protected IDomainGlossary Glossary { get; } = glossary;
    
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

    /// <summary>
    /// Use to map / constitute domain model objects where one row represents the entire data needed
    /// (e.g. from objects without or only with one-to-one links) 
    /// </summary>
    /// <param name="command">The SQL command that returns the result set in a DbDataReader</param>
    /// <param name="mapper">A simple delegate that constitutes the objects of type TModel from</param>
    protected async Task<IReadOnlyCollection<TModel>> ExecuteMapperAsync<TModel>(
        NpgsqlCommand command, Func<DbDataReader, IDomainGlossary, TModel> mapper)
    {
        var builder = ImmutableList.CreateBuilder<TModel>();
        
        await ExecuteReaderCoreAsync(command, async (reader, glossary) =>
        {
            while (await reader.ReadAsync())
            {
                builder.Add(mapper(reader, glossary));
            }
        });
        return builder.ToImmutable();
    }

    /// <summary>
    /// Use to map / constitute domain model objects where multiple rows need to be aggregated into single objects
    /// (e.g. entities with collections via one-to-many relationships) 
    /// </summary>
    /// <param name="command">The SQL command that returns the denormalized result set in a DbDataReader</param>
    /// <param name="keyGetter">Identifies which rows belong together</param>
    /// <param name="modelInitializer">Creates the object of type TModel with empty collections</param>
    /// <param name="accumulateData">Adds data from each row to the collections</param>
    /// <param name="modelFinalizer">Converts mutable collections to immutable ones</param>
    protected async Task<IReadOnlyCollection<TModel>> ExecuteMapperAsync<TModel, TKey>(
        NpgsqlCommand command, 
        Func<DbDataReader, TKey> keyGetter,
        Func<DbDataReader, TModel> modelInitializer,
        Action<TModel, DbDataReader> accumulateData,
        Func<TModel, TModel> modelFinalizer)
    {
        var builder = ImmutableList.CreateBuilder<TModel>();

        await ExecuteReaderCoreAsync(command, async (reader, _) =>
        {
            TModel? currentModel = default;
            TKey? currentKey = default;

            while (await reader.ReadAsync())
            {
                var key = keyGetter(reader);

                if (!Equals(key, currentKey))
                {
                    FinalizeCurrentModel();
                    currentModel = modelInitializer(reader);
                    currentKey = key;
                }

                accumulateData(currentModel!, reader);
            }

            FinalizeCurrentModel();
            return;

            void FinalizeCurrentModel()
            {
                if (currentModel != null)
                    builder.Add(modelFinalizer(currentModel));
            }
        });

        return builder.ToImmutable();
    }

    private async Task ExecuteReaderCoreAsync(
        NpgsqlCommand command,
        Func<DbDataReader, IDomainGlossary, Task> processReader)
    {
        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;

            await using var reader = await command.ExecuteReaderAsync();
            await processReader(reader, Glossary);
        });
    }
}