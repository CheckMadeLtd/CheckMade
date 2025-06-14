using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;

public interface IDerivedWorkflowBridgesRepository
{
    Task<WorkflowBridge?> GetAsync(TlgChatId dstChatId, TlgMessageId dstMessageId);
    Task<IReadOnlyCollection<WorkflowBridge>> GetAllAsync(ILiveEventInfo liveEvent);
}