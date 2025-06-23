using System.Collections.Concurrent;
using System.Data.Common;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Services.Persistence.JsonHelpers;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Constitutors.Utils;

namespace CheckMade.Services.Persistence.Constitutors;

public sealed class InputsConstitutor
{
    private readonly ConcurrentDictionary<HistoricInputIdentifier, Input> _inputsByIdCache = new();
    
    internal Input ConstituteInput(
        DbDataReader reader, 
        Option<IRoleInfo> roleInfo,
        Option<ILiveEventInfo> liveEventInfo,
        IDomainGlossary glossary)
    {
        var timeStamp = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("input_date"));
        MessageId messageId = reader.GetInt32(reader.GetOrdinal("input_message_id"));
        var identifier = new HistoricInputIdentifier(messageId, timeStamp);

        if (!_inputsByIdCache.TryGetValue(identifier, out var input))
        {
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
            
            input = new Input(
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

            _inputsByIdCache.TryAdd(identifier, input);
        }
        
        return input;

        Option<ResultantWorkflowState> GetWorkflowInfo()
        {
            if (reader.IsDBNull(reader.GetOrdinal("input_workflow")))
                return Option<ResultantWorkflowState>.None();
            
            return new ResultantWorkflowState(
                reader.GetString(reader.GetOrdinal("input_workflow")),
                reader.GetString(reader.GetOrdinal("input_wf_state")));
        }
    }
}