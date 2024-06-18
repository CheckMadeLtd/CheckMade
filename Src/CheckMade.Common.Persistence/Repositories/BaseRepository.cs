using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
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

    protected static Option<T> GetOption<T>(DbDataReader reader, int ordinal)
    {
        var valueRaw = reader.GetValue(ordinal);

        if (typeof(T) == typeof(EmailAddress) && valueRaw != DBNull.Value)
        {
            // ToDo: try removing the (object) cast in the middle. Should work??
            return (Option<T>) (object) Option<EmailAddress>.Some(
                new EmailAddress(reader.GetFieldValue<string>(ordinal)));
        }
        
        return valueRaw != DBNull.Value
            ? Option<T>.Some(reader.GetFieldValue<T>(ordinal))
            : Option<T>.None();
    }
}