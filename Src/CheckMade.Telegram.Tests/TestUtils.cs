using Telegram.Bot.Types;
using ChatAlias = Telegram.Bot.Types.Chat;

namespace CheckMade.Telegram.Tests;

internal static class TestUtils
{
    internal static Message GetValidTestMessage() =>
        new Message
        {
            Text = "Hello World Test", 
            Chat = new ChatAlias { Id = 123456789 }, 
            From = new User { Id = 987654321 }
        };
}