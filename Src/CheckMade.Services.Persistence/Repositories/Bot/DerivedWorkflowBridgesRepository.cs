using System.Data.Common;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Constitutors.StaticConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Bot;

public sealed class DerivedWorkflowBridgesRepository(
    IDbExecutionHelper dbHelper, 
    IDomainGlossary glossary,
    IInputsRepository inputRepo) 
    : BaseRepository(dbHelper, glossary), IDerivedWorkflowBridgesRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<WorkflowBridge>> _cache = Option<IReadOnlyCollection<WorkflowBridge>>.None();
    
    private readonly Func<DbDataReader, IDomainGlossary, WorkflowBridge> _workflowBridgeMapper =
        (reader, glossary) =>
        {
            var sourceInput = inputRepo.InputMapper(reader, glossary);

            return ConstituteWorkflowBridge(reader, sourceInput);
        };

    private const string GetBaseQuery = """
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
                                        INNER JOIN inputs inp on dwb.src_input_id = inp.id
                                        LEFT JOIN roles r on inp.role_id = r.id
                                        LEFT JOIN live_events le on inp.live_event_id = le.id
                                        LEFT JOIN derived_workflow_states dws on dws.inputs_id = inp.id
                                        """;

    public async Task<WorkflowBridge?> GetAsync(ChatId dstChatId, MessageId dstMessageId)
    {
        const string whereClause = "WHERE dwb.dst_chat_id = @dstChatId AND dwb.dst_message_id = @dstMessageId";

        var normalParameters = new Dictionary<string, object>
        {
            ["@dstChatId"] = dstChatId.Id,
            ["dstMessageId"] = dstMessageId.Id
        };
        
        var command = GenerateCommand($"{GetBaseQuery}\n{whereClause}", normalParameters);

        return (await 
                ExecuteMapperAsync(command, _workflowBridgeMapper))
            .FirstOrDefault();
    }
    
    public async Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent)
    {
        if (_cache.IsNone)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (_cache.IsNone)
                {
                    const string whereClause = "WHERE le.name = @liveEventName";

                    var normalParameters = new Dictionary<string, object>
                    {
                        ["@liveEventName"] = liveEvent.Name
                    };
                    
                    var command = GenerateCommand($"{GetBaseQuery}\n{whereClause}", normalParameters);

                    var bridges = await ExecuteMapperAsync(command, _workflowBridgeMapper);
                    
                    _cache = Option<IReadOnlyCollection<WorkflowBridge>>.Some(bridges);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cache.GetValueOrThrow();
    }
    
    // private void EmptyCache() => _cache = Option<IReadOnlyCollection<WorkflowBridge>>.None();
}