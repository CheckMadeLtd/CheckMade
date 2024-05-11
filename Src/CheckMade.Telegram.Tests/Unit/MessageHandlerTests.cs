using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

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
    public async Task HandleMessageAsync_SendsCorrectEchoMessageByBotType_ForValidTextMessage(
        string inputText, BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var textMessage = GetValidTextMessage(inputText);
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot {botType}: {inputText}";

        // Act
        await handler.HandleMessageAsync(textMessage, botType);
        
        // Assert
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                textMessage.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_SendsCorrectEchoMessage_ForValidPhotoMessageToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var photoMessage = GetValidPhotoMessage();
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot Submissions: photo";
        
        // Act
        await handler.HandleMessageAsync(photoMessage, BotType.Submissions);
        
        // Assert
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                photoMessage.Chat.Id,
                expectedOutputMessage, 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_LogsWarningAndReturns_ForUnhandledMessageType()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var unknownMessage = new Message // type 'Unknown' is derived by Telegram for lack of any props!
        {
            Chat = new Chat { Id = 123L }
        };

        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarningMessage}";

        var mockLogger = new Mock<ILogger<MessageHandler>>();
        mockLogger.Setup(l => l.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!))
            .Verifiable();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        
        _services = serviceCollection.BuildServiceProvider();
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(unknownMessage, BotType.Submissions);
        
        // Assert
        mockLogger.Verify();
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
        
        mockBotClientWrapper
            .Setup(x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                expectedErrorMessage, 
                It.IsAny<CancellationToken>()))
            .Verifiable();

        var textMessage = GetValidTextMessage("some valid text");
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(textMessage, BotType.Submissions);
        
        // Assert
        mockBotClientWrapper.Verify();
    }

    private static Message GetValidTextMessage(string inputText) => 
        new()
        {
            From = new User { Id = 1234L },
            Chat = new Chat { Id = 4321L },
            Date = DateTime.Now,
            Text = inputText
        };

    private static Message GetValidPhotoMessage() => 
        new()
        {
            From = new User { Id = 1234L },
            Chat = new Chat { Id = 4321L },
            Date = DateTime.Now,
            Photo = [new PhotoSize{ Height = 1, Width = 1 }]
        };
}
