using CheckMade.Common.Interfaces.Utils;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    InputMessage GetValidModelInputTextMessage();
}

internal class TestUtils(IRandomizer randomizer) : ITestUtils
{
    public InputMessage GetValidModelInputTextMessage() =>
        new(randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                $"Hello World, Valid Test: {randomizer.GenerateRandomLong()}"
                ));
}