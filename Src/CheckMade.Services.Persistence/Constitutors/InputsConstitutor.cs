using System.Collections.Concurrent;
using System.Data.Common;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Services.Persistence.JsonHelpers;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Constitutors.Utils;

namespace CheckMade.Services.Persistence.Constitutors;

public sealed class InputsConstitutor
{
    /* IMPORTANT: We are caching only InputDetails, not the entire Input object, because inputs can get updated
     * during a single request AFTER they have been constituted here (e.g. via UpdateGuid()).
     * This leads to an outdated input objects and Exceptions, as debugged on 23/06/2025.
     * While this means occasionally redundant constitutions of Input objects, the performance bottleneck are the
     * Json deserializations, so having those cached still solves the performance problem.
     *
     * ASSUMPTION: Other than e.g. EntityGuid, the InputDetails never get updated once they are saved in the db.  
    */ 
    private readonly ConcurrentDictionary<int, InputDetails> _detailsByInputIdCache = new();
    
    internal Input ConstituteInput(
        DbDataReader reader, 
        Option<IRoleInfo> roleInfo,
        Option<ILiveEventInfo> liveEventInfo,
        IDomainGlossary glossary)
    {
        var id = reader.GetFieldValue<int>(reader.GetOrdinal("input_id"));
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
        
        if (!_detailsByInputIdCache.TryGetValue(id, out var details))
        {
            var rawDetails = reader.GetString(reader.GetOrdinal("input_details"));
            details = JsonHelper.DeserializeFromJson<InputDetails>(rawDetails, glossary) 
                      ?? throw new InvalidDataException($"Failed to deserialize '{nameof(InputDetails)}'!");

            _detailsByInputIdCache.TryAdd(id, details);
        }
        
        return new Input(
            id,
            timeStamp,
            messageId,
            new Agent(userId, chatId, interactionMode),
            inputType,
            roleInfo,
            liveEventInfo,
            resultantWorkflow,
            guid,
            Option<string>.None(), 
            details);

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