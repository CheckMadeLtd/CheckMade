// ToDo: Implementing this test is more complicated than I thought, will leave for later. 
// See: https://chat.openai.com/share/e9f1388e-d789-4c41-9637-2f4ffe5d4cb1




// using System.Net;
// using System.Text;
// using CheckMade.Telegram.Function.Endpoints;
// using CheckMade.Telegram.Function.Services;
// using CheckMade.Telegram.Tests.Startup;
// using FluentAssertions;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Newtonsoft.Json;
// using Telegram.Bot.Types;
//
// namespace CheckMade.Telegram.Tests.Integration;
//
// public class TelegramFunctionTests(IntegrationTestStartup setup) : IClassFixture<IntegrationTestStartup>
// {
//     private readonly ServiceProvider _services = setup.ServiceProvider;
    
    // [Theory]
    // [InlineData(BotType.Submissions)]
    // [InlineData(BotType.Communications)]
    // [InlineData(BotType.Notifications)]
    
    // [Fact]    
    // public async Task SubmissionsBot_RespondsWithOk_ForValidUpdate()
    // {
    //     var mockLoggerBot = new Mock<ILogger<SubmissionsBot>>().Object;
    //     var updateHandler = _services.GetRequiredService<IBotUpdateHandler>();
    //     var submissionsBot = new SubmissionsBot(mockLoggerBot, updateHandler);
    //     
    //     const long validUserId = 123L;
    //     const string validText = "Valid text message";
    //     var now = DateTime.Now;
    //
    //     var update = new Update
    //     {
    //         Message = new Message
    //         {
    //             From = new User
    //             {
    //                 Id = validUserId,
    //                 IsBot = false,
    //                 FirstName = "Daniel"
    //             },
    //             Date = now,
    //             Text = validText,
    //             Chat = new Chat()
    //         }
    //     };
    //
    //     var jsonString = JsonConvert.SerializeObject(update);
    //     var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
    //
    //     var requestMock = new Mock<HttpRequestData>();
    //     requestMock.Setup(x => x.Body).Returns(stream);
    //
    //     var response = await submissionsBot.Run(requestMock.Object);
    //
    //     response.Should().Be(HttpStatusCode.OK);
    // }
// }