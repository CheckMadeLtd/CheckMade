using CheckMade.Common.DomainModel.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Interfaces.ChatBotFunction;

/// <summary>
/// Thread-safe cache for tracking the last message ID sent by each bot to each TlgAgent.
/// Avoids the need for the previous, dangerous messageId-1 pattern by storing actual sent message IDs.
/// See background in: https://github.com/CheckMadeLtd/CheckMade/issues/294
/// Used e.g. to edit Bot's previous output, e.g. for prompt finalization. 
/// </summary>
public interface ILastOutputMessageIdCache
{
    void UpdateLastMessageId(TlgAgent tlgAgent, TlgMessageId messageId);
    Option<TlgMessageId> GetLastMessageId(TlgAgent tlgAgent);
    bool RemoveLastMessageId(TlgAgent tlgAgent);
    int CacheSize { get; }
} 
