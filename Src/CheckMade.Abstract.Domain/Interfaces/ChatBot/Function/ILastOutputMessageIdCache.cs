using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Interfaces.ChatBot.Function;

/// <summary>
/// Thread-safe cache for tracking the last message ID sent by each bot to each Agent.
/// Avoids the need for the previous, dangerous messageId-1 pattern by storing actual sent message IDs.
/// See background in: https://github.com/CheckMadeLtd/CheckMade/issues/294
/// Used e.g. to edit Bot's previous output, e.g. for prompt finalization. 
/// </summary>
public interface ILastOutputMessageIdCache
{
    void UpdateLastMessageId(Agent agent, MessageId messageId);
    Option<MessageId> GetLastMessageId(Agent agent);
    bool RemoveLastMessageId(Agent agent);
    int CacheSize { get; }
} 
