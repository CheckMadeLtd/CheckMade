using System.Collections.Concurrent;
using CheckMade.Common.Interfaces.ChatBotFunction;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

public sealed class LastOutputMessageIdCache : ILastOutputMessageIdCache
{
    private readonly ConcurrentDictionary<TlgAgent, TlgMessageId> _lastMessageIdsByTlgAgent = new();

    /// <summary>
    /// WARNING: Do NOT inject scoped services into this singleton!
    /// See: https://github.com/CheckMadeLtd/CheckMade/wiki/Dev-Style-Guide-And-Pitfalls#avoid-singletons-that-hold-on-to-scoped-services
    /// This constructor should only accept stateless/configuration dependencies, if any.
    /// </summary>
    public LastOutputMessageIdCache()
    {
    }
    
    public void UpdateLastMessageId(TlgAgent tlgAgent, TlgMessageId messageId) =>
        _lastMessageIdsByTlgAgent.AddOrUpdate(
            tlgAgent, messageId, (_, _) => messageId);

    public Option<TlgMessageId> GetLastMessageId(TlgAgent tlgAgent) =>
        _lastMessageIdsByTlgAgent.TryGetValue(tlgAgent, out var messageId) 
            ? Option<TlgMessageId>.Some(messageId)
            : Option<TlgMessageId>.None();

    public bool RemoveLastMessageId(TlgAgent tlgAgent) =>
        _lastMessageIdsByTlgAgent.TryRemove(tlgAgent, out _);

    public int CacheSize => _lastMessageIdsByTlgAgent.Count;
}