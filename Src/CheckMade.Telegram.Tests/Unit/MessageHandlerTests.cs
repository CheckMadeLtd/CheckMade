using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class MessageHandlerTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("Normal valid text message", BotType.Submissions)]
    [InlineData("Normal valid text message", BotType.Communications)]
    [InlineData("Normal valid text message", BotType.Notifications)]
    [InlineData("_", BotType.Submissions)]
    [InlineData(" valid text message \n with line break and trailing spaces ", BotType.Submissions)]
    public async Task HandleMessageAsync_SendsCorrectEchoMessageByBotType_ForValidInputMessage(
        string inputText, BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var telegramMessage = GetValidTelegramMessage(inputText);
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act
        await handler.HandleMessageAsync(telegramMessage, botType);
        
        // Assert
        var expectedOutputMessage = $"Echo from bot {botType}: {inputText}";
        
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                telegramMessage.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var mockSubmissionsRequestProcessor = new Mock<ISubmissionsRequestProcessor>();
        mockSubmissionsRequestProcessor
            .Setup<Task<string>>(rp => rp.EchoAsync(It.IsAny<InputMessage>()))
            .Throws(new DataAccessException("Mock DataAccess Error", new Exception()));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        mockRequestProcessorSelector
            .Setup(rps => rps.GetRequestProcessor(BotType.Submissions))
            .Returns(mockSubmissionsRequestProcessor.Object);
        
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => mockRequestProcessorSelector.Object);
        
        _services = serviceCollection.BuildServiceProvider();

        const string expectedErrorMessage = $"{MessageHandler.DataAccessExceptionErrorMessageStub} " +
                                            $"{MessageHandler.CallToActionMessageAfterErrorReport}";
        
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        
        var outputMessageResult = string.Empty;
        mockBotClientWrapper
            .Setup(x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, CancellationToken>((_, outputMessage, _) => 
                outputMessageResult = outputMessage);
        
        var telegramMessage = GetValidTelegramMessage("some valid text");
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(telegramMessage, BotType.Submissions);
        
        // Assert
        outputMessageResult.Should().Be(expectedErrorMessage);
    }

    private static Message GetValidTelegramMessage(string inputText) => 
        new()
        {
            From = new User { Id = 1234L },
            Chat = new Chat { Id = 4321L },
            Date = DateTime.Now,
            Text = inputText
        };
}
