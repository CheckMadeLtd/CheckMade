using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Persistence.JsonHelpers;
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
    
    protected static (
        Func<DbDataReader, int> getKey,
        Func<DbDataReader, User> initializeModel,
        Action<User, DbDataReader> accumulateData,
        Func<User, User> finalizeModel) GetUserReader()
    {
        return (
            getKey: reader => reader.GetInt32(reader.GetOrdinal("user_id")),
            initializeModel: reader => new User(ConstituteUserInfo(reader), new List<IRoleInfo>()),
            accumulateData: (user, reader) =>
            {
                var roleInfo = ConstituteRoleInfo(reader);
                if (roleInfo.IsSome)
                {
                    ((List<IRoleInfo>)user.HasRoles).Add(roleInfo.GetValueOrThrow());
                }
            },
            finalizeModel: user => user with { HasRoles = user.HasRoles.ToImmutableReadOnlyCollection() }
        );
    }
    
    protected static readonly Func<DbDataReader, Role> ReadRole = reader =>
    {
        var userInfo = ConstituteUserInfo(reader);
        var liveEventInfo = ConstituteLiveEventInfo(reader);

        return ConstituteRole(reader, userInfo, liveEventInfo.GetValueOrThrow());
    };

    protected static readonly Func<DbDataReader, TlgInput> ReadTlgInput = reader =>
    {
        var originatorRoleInfo = ConstituteRoleInfo(reader);
        var liveEventInfo = ConstituteLiveEventInfo(reader);
        
        return ConstituteTlgInput(reader, originatorRoleInfo, liveEventInfo);
    };

    protected static readonly Func<DbDataReader, TlgAgentRoleBind> ReadTlgAgentRoleBind = reader =>
    {
        var role = ReadRole(reader);
        var tlgAgent = ConstituteTlgAgent(reader);

        return ConstituteTlgAgentRoleBind(reader, role, tlgAgent);
    };

    private static IUserInfo ConstituteUserInfo(DbDataReader reader)
    {
        return new UserInfo(
            new MobileNumber(reader.GetString(reader.GetOrdinal("user_mobile"))),
            reader.GetString(reader.GetOrdinal("user_first_name")),
            GetOption<string>(reader, reader.GetOrdinal("user_middle_name")),
            reader.GetString(reader.GetOrdinal("user_last_name")),
            GetOption<EmailAddress>(reader, reader.GetOrdinal("user_email")),
            EnsureEnumValidityOrThrow(
                (LanguageCode)reader.GetInt16(reader.GetOrdinal("user_language"))),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("user_status"))));
    }

    private static LiveEventVenue ConstituteLiveEventVenue(DbDataReader reader)
    {
        return new LiveEventVenue(
            reader.GetString(reader.GetOrdinal("venue_name")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("venue_status"))));
    }

    private static LiveEvent ConstituteLiveEvent(
        DbDataReader reader,
        IReadOnlyCollection<IRoleInfo> roles,
        LiveEventVenue venue,
        IReadOnlyCollection<ISphereOfAction> spheres) =>
        new(
            ConstituteLiveEventInfo(reader).GetValueOrThrow(),
            roles,
            venue,
            spheres);
    
    private static Option<ILiveEventInfo> ConstituteLiveEventInfo(DbDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("live_event_name")))
            return Option<ILiveEventInfo>.None();
        
        return new LiveEventInfo(
            reader.GetString(reader.GetOrdinal("live_event_name")),
            reader.GetDateTime(reader.GetOrdinal("live_event_start_date")),
            reader.GetDateTime(reader.GetOrdinal("live_event_end_date")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("live_event_status"))));
    }

    private static Role ConstituteRole(DbDataReader reader, IUserInfo userInfo, ILiveEventInfo liveEventInfo) =>
        new(ConstituteRoleInfo(reader).GetValueOrThrow(),
            userInfo,
            liveEventInfo);

    private static Option<IRoleInfo> ConstituteRoleInfo(DbDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("role_token")))
            return Option<IRoleInfo>.None();
        
        return new RoleInfo(
            reader.GetString(reader.GetOrdinal("role_token")),
            EnsureEnumValidityOrThrow(
                (RoleType)reader.GetInt16(reader.GetOrdinal("role_type"))),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status"))));
    }

    private static TlgInput ConstituteTlgInput(
        DbDataReader reader, Option<IRoleInfo> roleInfo, Option<ILiveEventInfo> liveEventInfo)
    {
        TlgUserId tlgUserId = reader.GetInt64(reader.GetOrdinal("input_user_id"));
        TlgChatId tlgChatId = reader.GetInt64(reader.GetOrdinal("input_chat_id"));
        var interactionMode = EnsureEnumValidityOrThrow(
            (InteractionMode)reader.GetInt16(reader.GetOrdinal("input_mode")));
        var tlgInputType = EnsureEnumValidityOrThrow(
            (TlgInputType)reader.GetInt16(reader.GetOrdinal("input_type")));
        var tlgDetails = reader.GetString(reader.GetOrdinal("input_details"));

        return new TlgInput(
            new TlgAgent(tlgUserId, tlgChatId, interactionMode),
            tlgInputType,
            roleInfo,
            liveEventInfo,
            JsonHelper.DeserializeFromJsonStrict<TlgInputDetails>(tlgDetails)
            ?? throw new InvalidOperationException("Failed to deserialize"));
    }

    private static TlgAgent ConstituteTlgAgent(DbDataReader reader)
    {
        return new TlgAgent(
            reader.GetInt64(reader.GetOrdinal("tarb_tlg_user_id")),
            reader.GetInt64(reader.GetOrdinal("tarb_tlg_chat_id")),
            EnsureEnumValidityOrThrow(
                (InteractionMode)reader.GetInt16(reader.GetOrdinal("tarb_interaction_mode"))));
    }

    private static TlgAgentRoleBind ConstituteTlgAgentRoleBind(DbDataReader reader, Role role, TlgAgent tlgAgent)
    {
        var activationDate = reader.GetDateTime(reader.GetOrdinal("tarb_activation_date"));

        var deactivationDateOrdinal = reader.GetOrdinal("tarb_deactivation_date");

        var deactivationDate = !reader.IsDBNull(deactivationDateOrdinal)
            ? Option<DateTime>.Some(reader.GetDateTime(deactivationDateOrdinal))
            : Option<DateTime>.None();

        var status = EnsureEnumValidityOrThrow(
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("tarb_status")));

        return new TlgAgentRoleBind(role, tlgAgent, activationDate, deactivationDate, status);
    }
    
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

    protected static TEnum EnsureEnumValidityOrThrow<TEnum>(TEnum uncheckedEnum) where TEnum : Enum
    {
        if (!EnumChecker.IsDefined(uncheckedEnum))
            throw new InvalidDataException($"The value {uncheckedEnum} for enum of type {typeof(TEnum)} is invalid. " + 
                                           $"Forgot to migrate data in db?");
        
        return uncheckedEnum;
    }
}