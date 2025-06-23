using System.Data.Common;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using static CheckMade.Services.Persistence.Constitutors.Utils;

namespace CheckMade.Services.Persistence.Constitutors;

public static class StaticConstitutors
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

    internal static Option<IRoleInfo> ConstituteRoleInfo(DbDataReader reader, IDomainGlossary glossary)
    {
        if (reader.IsDBNull(reader.GetOrdinal("role_token")))
            return Option<IRoleInfo>.None();

        var roleTypeTypeInfo = GetRoleTypeTypeInfo();
 
        if (!RoleTypeFactoryByFullTypeName.TryGetValue(roleTypeTypeInfo.FullName!, out var factory))
        {
            throw new InvalidOperationException($"Unhandled role type: {roleTypeTypeInfo.FullName}");
        }

        var roleType = factory();
        
        var result = new RoleInfo(
            reader.GetString(reader.GetOrdinal("role_token")),
            roleType,
            EnsureEnumValidityOrThrow(
                (DbRecordStatus)reader.GetInt16(reader.GetOrdinal("role_status")))); 

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
}