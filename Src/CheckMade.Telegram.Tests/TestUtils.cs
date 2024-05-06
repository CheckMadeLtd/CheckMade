using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal static class TestUtils
{
    internal static InputTextMessage GetValidTestMessage() =>
        new InputTextMessage(
            987654321,
            new MessageDetails(
                "Hello World Test"));
}