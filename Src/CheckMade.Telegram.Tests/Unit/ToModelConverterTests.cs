// using CheckMade.Telegram.Tests.Startup;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace CheckMade.Telegram.Tests.Unit;
//
// public class ToModelConverterTests
{
    // private ServiceProvider? _services;
    
    // [Theory]
    // [InlineData("")]
    // [InlineData(" ")]
    // [InlineData(null)]
    // public async Task HandleUpdateAsync_ThrowsArgumentNullException_ForEmptyMessageToSubmissionsBot(string inputText)
    // {
    //      _services = new UnitTestStartup().Services.BuildServiceProvider();
    
    //     // Arrange
    //     const BotType botType = BotType.Submissions;
    //     const long validUserId = 123L;
    //     const long validChatId = 321L;
    //     var now = DateTime.Now;
    //
    //     var update = new Update
    //     {
    //         Message = new Message
    //         {
    //             From = new User { Id = validUserId },
    //             Chat = new Chat { Id = validChatId },
    //             Date = now,
    //             Text = inputText
    //         }
    //     };
    //
    //     var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
    //     var handler = _services.GetRequiredService<IBotUpdateHandler>();
    //     
    //     // Act
    //     var actualOutputMessage = await handler.HandleUpdateAsync(update, botType);
    //     
    //     // Assert
    //     Action handleUpdate = () => handler.HandleUpdateAsync(update, botType);
    //     handleUpdate.Should().Throw<ArgumentNullException>();
    // }
}