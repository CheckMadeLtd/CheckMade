using System.Net;
using System.Text;
using CheckMade.Telegram.Function.Endpoints;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class TelegramFunctionTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
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

    // [Theory]
    // [InlineData(BotType.Submissions)]
    // [InlineData(BotType.Communications)]
    // [InlineData(BotType.Notifications)]
    
    [Fact]    
    public async Task SubmissionsBot_ProducesExpectedOutputMessage_ForValidMessageUpdate()
    {
        var mockLoggerBot = new Mock<ILogger<SubmissionsBot>>().Object;
        var updateHandler = _services.GetRequiredService<IBotUpdateHandler>();
        var submissionsBot = new SubmissionsBot(mockLoggerBot, updateHandler);
        
        const long validUserId = 123L;
        const string validText = "Valid text message";
        var now = DateTime.Now;

        var update = new Update
        {
            Message = new Message
            {
                From = new User
                {
                    Id = validUserId,
                    IsBot = false,
                    FirstName = "Daniel"
                },
                Date = now,
                Text = validText,
                Chat = new Chat()
            }
        };

        var jsonString = JsonConvert.SerializeObject(update);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

        var requestMock = new Mock<HttpRequestData>();

        requestMock.Setup(x => x.Body).Returns(stream);

        var response = await submissionsBot.Run(requestMock.Object);

        response.Should().Be(HttpStatusCode.OK);
    }
    
}