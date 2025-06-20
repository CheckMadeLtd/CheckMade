using System.Data.Common;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.Actors.RoleTypes;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using CheckMade.Services.Persistence.JsonHelpers;
using General.Utils.UiTranslation;
using General.Utils.Validators;

namespace CheckMade.Services.Persistence.Repositories;

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

        var jsonSw = System.Diagnostics.Stopwatch.StartNew();
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
        jsonSw.Stop();

        if (jsonSw.ElapsedMilliseconds > 1)
        {
            Console.WriteLine($"[PERF-DEBUG] for {nameof(ConstituteSphereOfAction)} " +
                              $"JSON: {jsonSw.ElapsedMilliseconds}ms");
        }

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
        
        var constructSw = System.Diagnostics.Stopwatch.StartNew();
        var result = new RoleInfo(
            reader.GetString(reader.GetOrdinal("role_token")),
            roleType,
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status")))); 
        constructSw.Stop();

        return result;

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

    internal static Input ConstituteInput(
        DbDataReader reader, 
        Option<IRoleInfo> roleInfo,
        Option<ILiveEventInfo> liveEventInfo,
        IDomainGlossary glossary)
    {
        var timeStamp = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("input_date"));
        MessageId messageId = reader.GetInt32(reader.GetOrdinal("input_message_id"));
        UserId userId = reader.GetInt64(reader.GetOrdinal("input_user_id"));
        ChatId chatId = reader.GetInt64(reader.GetOrdinal("input_chat_id"));
        var interactionMode = EnsureEnumValidityOrThrow(
            (InteractionMode)reader.GetInt16(reader.GetOrdinal("input_mode")));
        var inputType = EnsureEnumValidityOrThrow(
            (InputType)reader.GetInt16(reader.GetOrdinal("input_type")));
        var resultantWorkflow = GetWorkflowInfo();
        var guid = reader.IsDBNull(reader.GetOrdinal("input_guid"))
            ? Option<Guid>.None()
            : reader.GetGuid(reader.GetOrdinal("input_guid"));
        var inputDetails = reader.GetString(reader.GetOrdinal("input_details"));

        return new Input(
            timeStamp,
            messageId,
            new Agent(userId, chatId, interactionMode),
            inputType,
            roleInfo,
            liveEventInfo,
            resultantWorkflow,
            guid,
            Option<string>.None(), 
            JsonHelper.DeserializeFromJson<InputDetails>(inputDetails, glossary)
            ?? throw new InvalidDataException($"Failed to deserialize '{nameof(InputDetails)}'!"));

        Option<ResultantWorkflowState> GetWorkflowInfo()
        {
            if (reader.IsDBNull(reader.GetOrdinal("input_workflow")))
                return Option<ResultantWorkflowState>.None();
            
            return new ResultantWorkflowState(
                reader.GetString(reader.GetOrdinal("input_workflow")),
                reader.GetString(reader.GetOrdinal("input_wf_state")));
        }
    }

    internal static Agent ConstituteAgent(DbDataReader reader)
    {
        return new Agent(
            reader.GetInt64(reader.GetOrdinal("arb_user_id")),
            reader.GetInt64(reader.GetOrdinal("arb_chat_id")),
            EnsureEnumValidityOrThrow(
                (InteractionMode)reader.GetInt16(reader.GetOrdinal("arb_interaction_mode"))));
    }

    internal static AgentRoleBind ConstituteAgentRoleBind(DbDataReader reader, Role role, Agent agent)
    {
        var activationDate = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("arb_activation_date"));

        var deactivationDateOrdinal = reader.GetOrdinal("arb_deactivation_date");

        var deactivationDate = !reader.IsDBNull(deactivationDateOrdinal)
            ? Option<DateTimeOffset>.Some(reader.GetFieldValue<DateTimeOffset>(deactivationDateOrdinal))
            : Option<DateTimeOffset>.None();

        var status = EnsureEnumValidityOrThrow(
            (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("arb_status")));

        return new AgentRoleBind(role, agent, activationDate, deactivationDate, status);
    }

    internal static WorkflowBridge ConstituteWorkflowBridge(DbDataReader reader, Input sourceInput)
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