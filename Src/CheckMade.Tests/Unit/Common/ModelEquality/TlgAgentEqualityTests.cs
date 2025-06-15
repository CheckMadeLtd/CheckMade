using System.Collections.Concurrent;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;

namespace CheckMade.Tests.Unit.Common.ModelEquality;

public sealed class TlgAgentEqualityTests
{
    [Fact]
    public void TlgAgent_NestedStructuralEquality_WorksAsDictionaryKey()
    {
        // Arrange
        var userId = new UserId(12345);
        var chatId = new ChatId(67890);
        const InteractionMode mode = Operations;
    
        var agent1 = new TlgAgent(userId, chatId, mode);
        var agent2 = new TlgAgent(new UserId(12345), new ChatId(67890), Operations);
        var agent3 = new TlgAgent(new UserId(99999), chatId, mode); // Different user
    
        // Verify equality works correctly
        Assert.Equal(agent2, agent1);
        Assert.NotEqual(agent3, agent1);
    
        // Verify hash codes are consistent
        Assert.Equal(agent2.GetHashCode(), agent1.GetHashCode());
        Assert.NotEqual(agent3.GetHashCode(), agent1.GetHashCode());
    
        // Test actual dictionary usage
        var cache = new ConcurrentDictionary<TlgAgent, TlgMessageId>();
        var messageId = new TlgMessageId(555);
        
        cache[agent1] = messageId;
    
        // Retrieve with agent2 (structurally equal)
        Assert.True(cache.TryGetValue(agent2, out var retrievedId));
        Assert.Equal(messageId, retrievedId);
    
        // Should not find agent3
        Assert.False(cache.TryGetValue(agent3, out _));
    }
}