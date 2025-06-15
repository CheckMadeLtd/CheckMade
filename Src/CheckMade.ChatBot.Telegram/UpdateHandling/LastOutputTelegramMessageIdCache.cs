using System.Collections.Concurrent;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Telegram.UpdateHandling;

public sealed class LastOutputTelegramMessageIdCache : ILastOutputMessageIdCache
{
    private readonly ConcurrentDictionary<TlgAgent, TlgMessageId> _lastMessageIdsByTlgAgent = new();
    private ILogger<ILastOutputMessageIdCache> _logger;

    /// <summary>
    /// WARNING: Do NOT inject scoped services into this singleton!
    /// See: https://github.com/CheckMadeLtd/CheckMade/wiki/Dev-Style-Guide-And-Pitfalls#avoid-singletons-that-hold-on-to-scoped-services
    /// This constructor should only accept stateless/configuration dependencies, if any.
    /// </summary>
    public LastOutputTelegramMessageIdCache(ILogger<ILastOutputMessageIdCache> logger)
    {
        _logger = logger;
    }
    
    public void UpdateLastMessageId(TlgAgent tlgAgent, TlgMessageId messageId) =>
        _lastMessageIdsByTlgAgent.AddOrUpdate(
            tlgAgent, messageId, (_, _) => messageId);

    public Option<TlgMessageId> GetLastMessageId(TlgAgent tlgAgent)
    {
        var lastId = _lastMessageIdsByTlgAgent.TryGetValue(tlgAgent, out var messageId) 
            ? Option<TlgMessageId>.Some(messageId)
            : Option<TlgMessageId>.None();
        
        _logger.LogDebug(lastId.Match(
            static id => $"{nameof(LastOutputTelegramMessageIdCache)} hit with id: {id}",
            static () => $"No hit for {nameof(LastOutputTelegramMessageIdCache)}"));

        return lastId;
    }
        
    public bool RemoveLastMessageId(TlgAgent tlgAgent) =>
        _lastMessageIdsByTlgAgent.TryRemove(tlgAgent, out _);

    public int CacheSize => _lastMessageIdsByTlgAgent.Count;
}