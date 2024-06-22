using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.Generic;
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

    protected static readonly Func<DbDataReader, Role> ReadRole = reader =>
    {
        var user = new User(
            new MobileNumber(reader.GetString(reader.GetOrdinal("user_mobile"))),
            reader.GetString(reader.GetOrdinal("user_first_name")),
            GetOption<string>(reader, reader.GetOrdinal("user_middle_name")),
            reader.GetString(reader.GetOrdinal("user_last_name")),
            GetOption<EmailAddress>(reader, reader.GetOrdinal("user_email")),
            GetValidLanguageCode((LanguageCode)reader.GetInt16(reader.GetOrdinal("user_language"))),
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("user_status")));

        var venue = new LiveEventVenue(
            reader.GetString(reader.GetOrdinal("venue_name")),
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("venue_status")));

        var liveEvent = new LiveEvent(
            reader.GetString(reader.GetOrdinal("live_event_name")),
            reader.GetDateTime(reader.GetOrdinal("live_event_start_date")),
            reader.GetDateTime(reader.GetOrdinal("live_event_end_date")),
            // We leave this list empty to avoid unnecessary circular references in our object graph
            new List<Role>(),
            venue,
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("live_event_status")));

        return new Role(
            reader.GetString(reader.GetOrdinal("role_token")),
            (RoleType)reader.GetInt16(reader.GetOrdinal("role_type")),
            user,
            liveEvent,
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status")));

    };

    private static Option<T> GetOption<T>(DbDataReader reader, int ordinal)
    {
        var valueRaw = reader.GetValue(ordinal);

        if (typeof(T) == typeof(EmailAddress) && valueRaw != DBNull.Value)
        {
            return (Option<T>) (object) Option<EmailAddress>.Some(
                new EmailAddress(reader.GetFieldValue<string>(ordinal)));
        }
        
        return valueRaw != DBNull.Value
            ? Option<T>.Some(reader.GetFieldValue<T>(ordinal))
            : Option<T>.None();
    }

    private static LanguageCode GetValidLanguageCode(LanguageCode code) =>
        EnumChecker.IsDefined(code)
            ? code
            : LanguageCode.en;
}