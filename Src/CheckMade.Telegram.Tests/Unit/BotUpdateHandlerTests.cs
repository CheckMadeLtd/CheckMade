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

public class BotUpdateHandlerTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("_")]
    [InlineData("Normal valid text message")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task HandleUpdateAsync_SendsCorrectOutputMessage_ForValidUpdateToSubmissionsBot(string inputText)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const BotType botType = BotType.Submissions;
        var update = GetValidUpdate(inputText);
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act
        await handler.HandleUpdateAsync(update, botType);
        
        // Assert
        var expectedOutputMessage = $"Echo: {inputText}";
        
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                update.Message!.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task HandleUpdateAsync_ThrowsException_ForEmptyMessageToSubmissionsBot()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const BotType botType = BotType.Submissions;
        var update = new Update { Message = null };
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act
        var handleUpdate = () => handler.HandleUpdateAsync(update, botType);
        
        // Assert
        await handleUpdate.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleUpdateAsync_Fails_ForUpdateOfUnhandledType(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var update = new Update { CallbackQuery = new CallbackQuery() };
        
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        var handleUpdate = () => handler.HandleUpdateAsync(update, botType);
        await handleUpdate.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleUpdateAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown(BotType botType)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var mockRequestProcessor = new Mock<IRequestProcessor>();
        mockRequestProcessor
            .Setup<Task<string>>(x => x.EchoAsync(It.IsAny<InputMessage>()))
            .Throws(new DataAccessException("Mock DataAccess Error", new Exception()));
        serviceCollection.AddScoped<IRequestProcessor>(_ => mockRequestProcessor.Object);
        
        _services = serviceCollection.BuildServiceProvider();

        const string expectedErrorMessage = $"{BotUpdateHandler.DataAccessExceptionErrorMessageStub} " +
                                            $"{BotUpdateHandler.CallToActionMessageAfterErrorReport}";
        
        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        
        var outputMessageResult = string.Empty;
        mockBotClientWrapper
            .Setup(x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, CancellationToken>((_, outputMessage, _) => 
                outputMessageResult = outputMessage);
        
        var update = GetValidUpdate("some valid text");
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act 
        await handler.HandleUpdateAsync(update, botType);
        
        // Assert
        outputMessageResult.Should().Be(expectedErrorMessage);
    }

    private static Update GetValidUpdate(string inputText) => 
        new()
        {
            Message = new Message
            {
                From = new User { Id = 1234L },
                Chat = new Chat { Id = 4321L },
                Date = DateTime.Now,
                Text = inputText
            }
        };
}
