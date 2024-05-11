using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
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
    [InlineData("_")]
    [InlineData("Normal valid text message")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task HandleMessageAsync_SendsCorrectOutputMessage_ForValidUpdateToSubmissionsBot(string inputText)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const BotType botType = BotType.Submissions;
        var message = GetValidMessage(inputText);
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act
        await handler.HandleMessageAsync(message, botType);
        
        // Assert
        var expectedOutputMessage = $"Echo: {inputText}";
        
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                message.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleMessageAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown(BotType botType)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var mockRequestProcessor = new Mock<IRequestProcessor>();
        mockRequestProcessor
            .Setup<Task<string>>(x => x.EchoAsync(It.IsAny<InputMessage>()))
            .Throws(new DataAccessException("Mock DataAccess Error", new Exception()));
        serviceCollection.AddScoped<IRequestProcessor>(_ => mockRequestProcessor.Object);
        
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
        
        var message = GetValidMessage("some valid text");
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(message, botType);
        
        // Assert
        outputMessageResult.Should().Be(expectedErrorMessage);
    }

    private static Message GetValidMessage(string inputText) => 
        new()
        {
            From = new User { Id = 1234L },
            Chat = new Chat { Id = 4321L },
            Date = DateTime.Now,
            Text = inputText
        };
}
