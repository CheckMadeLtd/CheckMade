using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal static class TestUtils
{
    internal static InputTextMessage GetValidTestMessage() =>
        new(new Random().Next(10000),
            new MessageDetails(
                $"Hello World, Valid Test: {new Random().Next(10000)}",
                DateTime.Now));
    
}