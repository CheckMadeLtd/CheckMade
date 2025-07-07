using System.Collections.Concurrent;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.Logging;

namespace CheckMade.Bot.Telegram.UpdateHandling;

public sealed class LastOutputTelegramMessageIdCache : ILastOutputMessageIdCache
{
    private readonly ConcurrentDictionary<Agent, MessageId> _lastMessageIdsByAgent = new();
    private readonly ILogger<ILastOutputMessageIdCache> _logger;

    /// <warning>
    /// Do NOT inject scoped services into this singleton to avoid 'Captive Dependencies'
    /// This constructor should only accept stateless/configuration dependencies, if any.
    /// </warning>
    public LastOutputTelegramMessageIdCache(ILogger<ILastOutputMessageIdCache> logger)
    {
        _logger = logger;
    }
    
    public void UpdateLastMessageId(Agent agent, MessageId messageId) =>
        _lastMessageIdsByAgent.AddOrUpdate(
            agent, messageId, (_, _) => messageId);

    public Option<MessageId> GetLastMessageId(Agent agent)
    {
        var lastId = _lastMessageIdsByAgent.TryGetValue(agent, out var messageId) 
            ? Option<MessageId>.Some(messageId)
            : Option<MessageId>.None();
        
        _logger.LogDebug(lastId.Match(
            static id => $"{nameof(LastOutputTelegramMessageIdCache)} hit with id: {id}",
            static () => $"No hit for {nameof(LastOutputTelegramMessageIdCache)}"));

        return lastId;
    }
        
    public bool RemoveLastMessageId(Agent agent) =>
        _lastMessageIdsByAgent.TryRemove(agent, out _);

    public int CacheSize => _lastMessageIdsByAgent.Count;
}