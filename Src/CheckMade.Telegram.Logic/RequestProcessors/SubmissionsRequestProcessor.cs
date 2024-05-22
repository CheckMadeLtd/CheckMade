using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return await Attempt<string>.RunAsync(async () =>
        {
            await repo.AddOrThrowAsync(inputMessage);

            var submissionsBotCommandMenu = new BotCommandMenus();
            
            return inputMessage.Details.SubmissionsBotCommand.Match(
                botCommand => $"Echo of a Submissions BotCommand: " +
                              $"{submissionsBotCommandMenu.SubmissionsBotCommandMenu
                                  .First(kvp => kvp.Key == botCommand)
                                  .Value.Command}",
                () => inputMessage.Details.AttachmentType.Match(
                    type => $"Echo from bot Submissions: {type}",
                    () => $"Echo from bot Submissions: {inputMessage.Details.Text.GetValueOrDefault()}"));
        });
    }
}
