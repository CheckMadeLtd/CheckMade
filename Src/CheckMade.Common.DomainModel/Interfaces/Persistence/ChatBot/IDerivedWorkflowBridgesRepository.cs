using CheckMade.Common.DomainModel.Data.ChatBot;
using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;

public interface IDerivedWorkflowBridgesRepository
{
    Task<WorkflowBridge?> GetAsync(TlgChatId dstChatId, TlgMessageId dstMessageId);
    Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent);
}