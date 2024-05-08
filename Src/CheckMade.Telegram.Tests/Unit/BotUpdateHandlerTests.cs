using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class BotUpdateHandlerTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;

    
    
    [Theory]
    [InlineData(null, "Valid text")]
    [InlineData(123L, null)]
    [InlineData(null, null)]
    public void ConvertMessage_ThrowsArgumentNullException_ForInvalidInputs(long? userId, string text)
    {
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        var telegramInputMessage = new Message
        {
            From = userId.HasValue ? new User { Id = userId.Value } : null,
            Text = text
        };
        
        Action convertMessage = () => converter.ConvertMessage(telegramInputMessage);
        convertMessage.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConvertToModel_ReturnsCorrectModel_ForValidInput()
    {
        const long validUserId = 123L;
        const string validText = "Valid text message";
        var now = DateTime.Now;
        
        var telegramInputMessage = new Message
        {
            From = new User { Id = validUserId },
            Date = now,
            Text = validText
        };
        
        var expectedModel = new InputTextMessage(validUserId, new MessageDetails(validText, now));
        var converter = _services.GetRequiredService<IToModelConverter>();

        var actualModel = converter.ConvertMessage(telegramInputMessage);
        expectedModel.Should().Be(actualModel);
    }
}