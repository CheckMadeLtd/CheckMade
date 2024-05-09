using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal static class TestUtils
{
    internal static InputTextMessage GetValidTestMessage() =>
        new(987654321L,
            new MessageDetails(
                $"Hello World, Valid Test: {new Random().Next(1000)}",
                DateTime.Now));
}