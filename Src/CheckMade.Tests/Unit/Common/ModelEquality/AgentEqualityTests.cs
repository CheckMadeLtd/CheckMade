using System.Collections.Concurrent;
using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;

namespace CheckMade.Tests.Unit.Common.ModelEquality;

public sealed class AgentEqualityTests
{
    [Fact]
    public void Agent_NestedStructuralEquality_WorksAsDictionaryKey()
    {
        // Arrange
        var userId = new UserId(12345);
        var chatId = new ChatId(67890);
        const InteractionMode mode = Operations;
    
        var agent1 = new Agent(userId, chatId, mode);
        var agent2 = new Agent(new UserId(12345), new ChatId(67890), Operations);
        var agent3 = new Agent(new UserId(99999), chatId, mode); // Different user
    
        // Verify equality works correctly
        Assert.Equal(agent2, agent1);
        Assert.NotEqual(agent3, agent1);
    
        // Verify hash codes are consistent
        Assert.Equal(agent2.GetHashCode(), agent1.GetHashCode());
        Assert.NotEqual(agent3.GetHashCode(), agent1.GetHashCode());
    
        // Test actual dictionary usage
        var cache = new ConcurrentDictionary<Agent, MessageId>();
        var messageId = new MessageId(555);
        
        cache[agent1] = messageId;
    
        // Retrieve with agent2 (structurally equal)
        Assert.True(cache.TryGetValue(agent2, out var retrievedId));
        Assert.Equal(messageId, retrievedId);
    
        // Should not find agent3
        Assert.False(cache.TryGetValue(agent3, out _));
    }
}