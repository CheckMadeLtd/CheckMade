using CheckMade.Core.Model.Bot.DTOs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.ServiceInterfaces.Bot;

/// <summary>  
/// Thread-safe cache for tracking the last message ID sent by each bot to each Agent.
/// Avoids the need for the previous, dangerous messageId-1 pattern by storing actual sent message IDs.
/// See background in: https://github.com/CheckMadeLtd/CheckMade/issues/294
/// Used e.g. to edit Bot's previous output, e.g. for prompt finalization.
/// </summary>
/// <warning>
/// Should NOT be relied on (e.g. for actual business logic) because it will occasionally be empty (e.g. after cache reset)!!
/// </warning>

public interface ILastOutputMessageIdCache
{
    void UpdateLastMessageId(Agent agent, MessageId messageId);
    Option<MessageId> GetLastMessageId(Agent agent);
    bool RemoveLastMessageId(Agent agent);
    int CacheSize { get; }
} 
