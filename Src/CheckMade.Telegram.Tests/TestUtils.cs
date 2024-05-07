using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal static class TestUtils
{
    internal static InputTextMessage GetValidTestMessage() =>
        new InputTextMessage(
            987654321L,
            new MessageDetails(
                "Hello World, Valid Test"));
}