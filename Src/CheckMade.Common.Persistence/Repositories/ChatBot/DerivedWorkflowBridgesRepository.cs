using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public sealed class DerivedWorkflowBridgesRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), IDerivedWorkflowBridgesRepository
{
    public Task<WorkflowBridge?> GetAsync(TlgChatId dstChatId, TlgMessageId dstMessageId)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent)
    {
        const string rawQuery = """
                                SELECT
                                
                                r.token AS role_token, 
                                r.role_type AS role_type, 
                                r.status AS role_status, 
                                    
                                le.name AS live_event_name, 
                                le.start_date AS live_event_start_date, 
                                le.end_date AS live_event_end_date, 
                                le.status AS live_event_status, 
                                    
                                dws.resultant_workflow AS input_workflow,
                                dws.in_state AS input_wf_state,
                                
                                inp.date AS input_date,
                                inp.message_id AS input_message_id,
                                inp.user_id AS input_user_id, 
                                inp.chat_id AS input_chat_id, 
                                inp.interaction_mode AS input_mode, 
                                inp.input_type AS input_type,
                                inp.details AS input_details,
                                inp.entity_guid AS input_guid,
                                
                                dwb.dst_chat_id AS bridge_chat_id,
                                dwb.dst_message_id AS bridge_message_id
                                
                                FROM derived_workflow_bridges dwb
                                INNER JOIN tlg_inputs inp on dwb.src_input_id = inp.id
                                LEFT JOIN roles r on inp.role_id = r.id
                                LEFT JOIN live_events le on inp.live_event_id = le.id
                                LEFT JOIN derived_workflow_states dws on dws.tlg_inputs_id = inp.id
                                WHERE le.name = @liveEventName
                                """;

        var normalParameters = new Dictionary<string, object>
        {
            ["@liveEventName"] = liveEvent.Name
        };
        
        var command = GenerateCommand(rawQuery, normalParameters);

        return await ExecuteReaderOneToOneAsync(
            command, 
            ModelReaders.ReadWorkflowBridge);
    }
}