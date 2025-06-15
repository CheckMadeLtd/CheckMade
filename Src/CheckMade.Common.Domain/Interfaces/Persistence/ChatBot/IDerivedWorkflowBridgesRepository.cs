using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;

public interface IDerivedWorkflowBridgesRepository
{
    Task<WorkflowBridge?> GetAsync(ChatId dstChatId, MessageId dstMessageId);
    Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent);
}