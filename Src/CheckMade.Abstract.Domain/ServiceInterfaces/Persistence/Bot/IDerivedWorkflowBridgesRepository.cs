using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;

public interface IDerivedWorkflowBridgesRepository
{
    Task<WorkflowBridge?> GetAsync(ChatId dstChatId, MessageId dstMessageId);
    Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent);
}