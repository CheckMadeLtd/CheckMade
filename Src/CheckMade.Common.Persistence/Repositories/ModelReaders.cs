using System.Data.Common;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors;
using CheckMade.Common.Model.Core.Actors.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Common.Utils.Generic;

namespace CheckMade.Common.Persistence.Repositories;

internal static class ModelReaders
{
    internal static readonly Func<DbDataReader, IDomainGlossary, Role> ReadRole = 
        static (reader, glossary) =>
        {
            var userInfo = ConstituteUserInfo(reader);
            var liveEventInfo = ConstituteLiveEventInfo(reader);

            return ConstituteRole(reader, userInfo, liveEventInfo.GetValueOrThrow(), glossary);
        };

    internal static readonly Func<DbDataReader, IDomainGlossary, TlgInput> ReadTlgInput = 
        static (reader, glossary) =>
        {
            var originatorRoleInfo = ConstituteRoleInfo(reader, glossary);
            var liveEventInfo = ConstituteLiveEventInfo(reader);
        
            return ConstituteTlgInput(reader, originatorRoleInfo, liveEventInfo, glossary);
        };

    internal static readonly Func<DbDataReader, IDomainGlossary, TlgAgentRoleBind> ReadTlgAgentRoleBind = 
        static (reader, glossary) =>
        {
            var role = ReadRole(reader, glossary);
            var tlgAgent = ConstituteTlgAgent(reader);

            return ConstituteTlgAgentRoleBind(reader, role, tlgAgent);
        };

    internal static readonly Func<DbDataReader, IDomainGlossary, Vendor> ReadVendor = 
        static (reader, _) => 
            ConstituteVendor(reader).GetValueOrThrow();

    internal static readonly Func<DbDataReader, IDomainGlossary, WorkflowBridge> ReadWorkflowBridge =
        static (reader, glossary) =>
        {
            var sourceInput = ReadTlgInput(reader, glossary);

            return ConstituteWorkflowBridge(reader, sourceInput);
        };

    internal static (
        Func<DbDataReader, int> getKey,
        Func<DbDataReader, User> initializeModel,
        Action<User, DbDataReader> accumulateData,
        Func<User, User> finalizeModel)
        GetUserReader(IDomainGlossary glossary)
    {
        return (
            getKey: static reader => reader.GetInt32(reader.GetOrdinal("user_id")),
            initializeModel: static reader => 
                new User(
                    ConstituteUserInfo(reader),
                    new HashSet<IRoleInfo>(),
                    ConstituteVendor(reader)),
            accumulateData: (user, reader) =>
            {
                var roleInfo = ConstituteRoleInfo(reader, glossary);
                if (roleInfo.IsSome)
                    ((HashSet<IRoleInfo>)user.HasRoles).Add(roleInfo.GetValueOrThrow());
            },
            finalizeModel: static user => user with { HasRoles = user.HasRoles.ToImmutableReadOnlyCollection() }
        );
    }
    
    internal static (
        Func<DbDataReader, int> getKey,
        Func<DbDataReader, LiveEvent> initializeModel,
        Action<LiveEvent, DbDataReader> accumulateData,
        Func<LiveEvent, LiveEvent> finalizeModel)
        GetLiveEventReader(IDomainGlossary glossary)
    {
        return (
            getKey: static reader => reader.GetInt32(reader.GetOrdinal("live_event_id")),
            initializeModel: static reader => 
                new LiveEvent(
                    ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                    new HashSet<IRoleInfo>(),
                    ConstituteLiveEventVenue(reader),
                    new HashSet<ISphereOfAction>()),
            accumulateData: (liveEvent, reader) =>
            {
                var roleInfo = ConstituteRoleInfo(reader, glossary);
                if (roleInfo.IsSome)
                    ((HashSet<IRoleInfo>)liveEvent.WithRoles).Add(roleInfo.GetValueOrThrow());

                var sphereOfAction = ConstituteSphereOfAction(reader, glossary);
                if (sphereOfAction.IsSome)
                    ((HashSet<ISphereOfAction>)liveEvent.DivIntoSpheres).Add(sphereOfAction.GetValueOrThrow());
            },
            finalizeModel: static liveEvent => liveEvent with
            {
                WithRoles = liveEvent.WithRoles.ToImmutableReadOnlyCollection(),
                DivIntoSpheres = liveEvent.DivIntoSpheres.ToImmutableReadOnlyCollection()
            }
        );
    }

    private static Option<Vendor> ConstituteVendor(DbDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("vendor_name")))
            return Option<Vendor>.None();
        
        return new Vendor(
            reader.GetString(reader.GetOrdinal("vendor_name")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("vendor_status"))));
    }
    
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

    private static Option<ILiveEventInfo> ConstituteLiveEventInfo(DbDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("live_event_name")))
            return Option<ILiveEventInfo>.None();
        
        return new LiveEventInfo(
            reader.GetString(reader.GetOrdinal("live_event_name")),
            reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("live_event_start_date")),
            reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("live_event_end_date")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("live_event_status"))));
    }

    private static Option<ISphereOfAction> ConstituteSphereOfAction(DbDataReader reader, IDomainGlossary glossary)
    {
        if (reader.IsDBNull(reader.GetOrdinal("sphere_name")))
            return Option<ISphereOfAction>.None();

        var trade = GetTrade();

        const string invalidTradeTypeException = $"""
                                                  This is not an existing '{nameof(trade)}' or we forgot to
                                                  implement a new type in method '{nameof(ConstituteSphereOfAction)}' 
                                                  """;

        var detailsJson = reader.GetString(reader.GetOrdinal("sphere_details"));
        
        ISphereOfActionDetails details = trade switch
        {
            SanitaryTrade => 
                JsonHelper.DeserializeFromJsonStrict<SanitaryCampDetails>(detailsJson, glossary)
                ?? throw new InvalidDataException($"Failed to deserialize '{nameof(SanitaryCampDetails)}'!"),
            SiteCleanTrade => 
                JsonHelper.DeserializeFromJsonStrict<SiteCleaningZoneDetails>(detailsJson, glossary)
                ?? throw new InvalidDataException($"Failed to deserialize '{nameof(SiteCleaningZoneDetails)}'!"),
            _ => 
                throw new InvalidOperationException(invalidTradeTypeException)
        };

        var sphereName = reader.GetString(reader.GetOrdinal("sphere_name"));

        ISphereOfAction sphere = trade switch
        {
            SanitaryTrade => 
                new SphereOfAction<SanitaryTrade>(sphereName, details),
            SiteCleanTrade => 
                new SphereOfAction<SiteCleanTrade>(sphereName, details),
            _ => 
                throw new InvalidOperationException(invalidTradeTypeException)
        };
        
        return Option<ISphereOfAction>.Some(sphere);

        ITrade GetTrade()
        {
            var tradeId = new CallbackId(reader.GetString(reader.GetOrdinal("sphere_trade")));
            var tradeType = glossary.TermById[tradeId].TypeValue;

            if (tradeType is null || 
                !tradeType.IsAssignableTo(typeof(ITrade)))
            {
                throw new InvalidDataException($"The '{nameof(tradeType)}': '{tradeType?.FullName}' of this sphere " +
                                               $"can't be determined.");
            }

            return (ITrade)Activator.CreateInstance(tradeType)!;
        }
    }
    
    private static Role ConstituteRole(
        DbDataReader reader,
        IUserInfo userInfo,
        ILiveEventInfo liveEventInfo,
        IDomainGlossary glossary) =>
        new(ConstituteRoleInfo(reader, glossary).GetValueOrThrow(),
            userInfo,
            liveEventInfo);

    private static Option<IRoleInfo> ConstituteRoleInfo(DbDataReader reader, IDomainGlossary glossary)
    {
        if (reader.IsDBNull(reader.GetOrdinal("role_token")))
            return Option<IRoleInfo>.None();

        Dictionary<string, Func<IRoleType>> roleTypeFactoryByFullTypeName = new()
        {
            [typeof(LiveEventAdmin).FullName!] = static () => new LiveEventAdmin(),
            [typeof(LiveEventObserver).FullName!] = static () => new LiveEventObserver(),
            
            [typeof(TradeAdmin<SanitaryTrade>).FullName!] = () => new TradeAdmin<SanitaryTrade>(),
            [typeof(TradeInspector<SanitaryTrade>).FullName!] = () => new TradeInspector<SanitaryTrade>(),
            [typeof(TradeEngineer<SanitaryTrade>).FullName!] = () => new TradeEngineer<SanitaryTrade>(),
            [typeof(TradeTeamLead<SanitaryTrade>).FullName!] = () => new TradeTeamLead<SanitaryTrade>(),
            [typeof(TradeObserver<SanitaryTrade>).FullName!] = () => new TradeObserver<SanitaryTrade>(),
            
            [typeof(TradeAdmin<SiteCleanTrade>).FullName!] = () => new TradeAdmin<SiteCleanTrade>(),
            [typeof(TradeInspector<SiteCleanTrade>).FullName!] = () => new TradeInspector<SiteCleanTrade>(),
            [typeof(TradeEngineer<SiteCleanTrade>).FullName!] = () => new TradeEngineer<SiteCleanTrade>(),
            [typeof(TradeTeamLead<SiteCleanTrade>).FullName!] = () => new TradeTeamLead<SiteCleanTrade>(),
            [typeof(TradeObserver<SiteCleanTrade>).FullName!] = () => new TradeObserver<SiteCleanTrade>(),
        };
        
        var roleTypeTypeInfo = GetRoleTypeTypeInfo();

        if (!roleTypeFactoryByFullTypeName.TryGetValue(roleTypeTypeInfo.FullName!, out var factory))
        {
            throw new InvalidOperationException($"Unhandled role type: {roleTypeTypeInfo.FullName}");
        }

        var roleType = factory();
        
        return new RoleInfo(
            reader.GetString(reader.GetOrdinal("role_token")),
            roleType,
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status"))));

        Type GetRoleTypeTypeInfo()
        {
            var roleTypeId = new CallbackId(reader.GetString(reader.GetOrdinal("role_type")));
            var roleTypeTypeRaw = glossary.TermById[roleTypeId].TypeValue;

            if (roleTypeTypeRaw is null ||
                !roleTypeTypeRaw.IsAssignableTo(typeof(IRoleType)))
            {
                throw new InvalidDataException($"The '{nameof(roleTypeTypeRaw)}': " +
                                               $"'{roleTypeTypeRaw?.FullName}' of this Role can't be determined.");
            }

            return roleTypeTypeRaw;
        }
    }

    private static TlgInput ConstituteTlgInput(
        DbDataReader reader, 
        Option<IRoleInfo> roleInfo,
        Option<ILiveEventInfo> liveEventInfo,
        IDomainGlossary glossary)
    {
        var tlgDate = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("input_date"));
        TlgMessageId tlgMessageId = reader.GetInt32(reader.GetOrdinal("input_message_id"));
        TlgUserId tlgUserId = reader.GetInt64(reader.GetOrdinal("input_user_id"));
        TlgChatId tlgChatId = reader.GetInt64(reader.GetOrdinal("input_chat_id"));
        var interactionMode = EnsureEnumValidityOrThrow(
            (InteractionMode)reader.GetInt16(reader.GetOrdinal("input_mode")));
        var tlgInputType = EnsureEnumValidityOrThrow(
            (TlgInputType)reader.GetInt16(reader.GetOrdinal("input_type")));
        var resultantWorkflow = GetWorkflowInfo();
        var guid = reader.IsDBNull(reader.GetOrdinal("input_guid"))
            ? Option<Guid>.None()
            : reader.GetGuid(reader.GetOrdinal("input_guid"));
        var tlgDetails = reader.GetString(reader.GetOrdinal("input_details"));

        return new TlgInput(
            tlgDate,
            tlgMessageId,
            new TlgAgent(tlgUserId, tlgChatId, interactionMode),
            tlgInputType,
            roleInfo,
            liveEventInfo,
            resultantWorkflow,
            guid,
            Option<string>.None(), 
            JsonHelper.DeserializeFromJsonStrict<TlgInputDetails>(tlgDetails, glossary)
            ?? throw new InvalidDataException($"Failed to deserialize '{nameof(TlgInputDetails)}'!"));

        Option<ResultantWorkflowState> GetWorkflowInfo()
        {
            if (reader.IsDBNull(reader.GetOrdinal("input_workflow")))
                return Option<ResultantWorkflowState>.None();
            
            return new ResultantWorkflowState(
                reader.GetString(reader.GetOrdinal("input_workflow")),
                reader.GetString(reader.GetOrdinal("input_wf_state")));
        }
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
        var activationDate = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("tarb_activation_date"));

        var deactivationDateOrdinal = reader.GetOrdinal("tarb_deactivation_date");

        var deactivationDate = !reader.IsDBNull(deactivationDateOrdinal)
            ? Option<DateTimeOffset>.Some(reader.GetFieldValue<DateTimeOffset>(deactivationDateOrdinal))
            : Option<DateTimeOffset>.None();

        var status = EnsureEnumValidityOrThrow(
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("tarb_status")));

        return new TlgAgentRoleBind(role, tlgAgent, activationDate, deactivationDate, status);
    }

    private static WorkflowBridge ConstituteWorkflowBridge(DbDataReader reader, TlgInput sourceInput)
    {
        var destinationChatId = reader.GetInt64(reader.GetOrdinal("bridge_chat_id"));
        var destinationMessageId = reader.GetInt32(reader.GetOrdinal("bridge_message_id"));

        return new WorkflowBridge(sourceInput, destinationChatId, destinationMessageId);
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

    private static TEnum EnsureEnumValidityOrThrow<TEnum>(TEnum uncheckedEnum) where TEnum : Enum
    {
        if (!EnumChecker.IsDefined(uncheckedEnum))
            throw new InvalidDataException($"The value {uncheckedEnum} for enum of type {typeof(TEnum)} is invalid. " + 
                                           $"Forgot to migrate data in db?");
        
        return uncheckedEnum;
    }
}