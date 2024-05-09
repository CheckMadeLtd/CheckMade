using CheckMade.Common.Interfaces.Utils;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    InputTextMessage GetValidTestMessage();
}

internal class TestUtils(IRandomizer randomizer) : ITestUtils
{
    public InputTextMessage GetValidTestMessage() =>
        new(randomizer.GenerateRandomLong(),
            new MessageDetails(
                $"Hello World, Valid Test: {randomizer.GenerateRandomLong()}",
                DateTime.Now));
}