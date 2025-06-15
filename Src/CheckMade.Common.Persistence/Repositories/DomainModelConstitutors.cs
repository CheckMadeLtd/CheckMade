using System.Data.Common;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Actors;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.Domain.Data.Core.LiveEvents;
using CheckMade.Common.Domain.Data.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Common.Utils.Validators;

namespace CheckMade.Common.Persistence.Repositories;

internal static class DomainModelConstitutors
{
    internal static Option<Vendor> ConstituteVendor(DbDataReader reader)
    {
        if (reader.IsDBNull(reader.GetOrdinal("vendor_name")))
            return Option<Vendor>.None();
        
        return new Vendor(
            reader.GetString(reader.GetOrdinal("vendor_name")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("vendor_status"))));
    }
    
    internal static IUserInfo ConstituteUserInfo(DbDataReader reader)
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

    internal static LiveEventVenue ConstituteLiveEventVenue(DbDataReader reader)
    {
        return new LiveEventVenue(
            reader.GetString(reader.GetOrdinal("venue_name")),
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("venue_status"))));
    }

    internal static Option<ILiveEventInfo> ConstituteLiveEventInfo(DbDataReader reader)
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

    internal static Option<ISphereOfAction> ConstituteSphereOfAction(DbDataReader reader, IDomainGlossary glossary)
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
                JsonHelper.DeserializeFromJson<SanitaryCampDetails>(detailsJson, glossary)
                ?? throw new InvalidDataException($"Failed to deserialize '{nameof(SanitaryCampDetails)}'!"),
            SiteCleanTrade => 
                JsonHelper.DeserializeFromJson<SiteCleaningZoneDetails>(detailsJson, glossary)
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

    internal static Option<IRoleInfo> ConstituteRoleInfo(DbDataReader reader, IDomainGlossary glossary)
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

    internal static TlgInput ConstituteTlgInput(
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
            JsonHelper.DeserializeFromJson<TlgInputDetails>(tlgDetails, glossary)
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

    internal static TlgAgent ConstituteTlgAgent(DbDataReader reader)
    {
        return new TlgAgent(
            reader.GetInt64(reader.GetOrdinal("tarb_tlg_user_id")),
            reader.GetInt64(reader.GetOrdinal("tarb_tlg_chat_id")),
            EnsureEnumValidityOrThrow(
                (InteractionMode)reader.GetInt16(reader.GetOrdinal("tarb_interaction_mode"))));
    }

    internal static TlgAgentRoleBind ConstituteTlgAgentRoleBind(DbDataReader reader, Role role, TlgAgent tlgAgent)
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

    internal static WorkflowBridge ConstituteWorkflowBridge(DbDataReader reader, TlgInput sourceInput)
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