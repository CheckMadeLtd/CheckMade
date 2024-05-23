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

            var botCommandMenus = new BotCommandMenus();

            // ToDo: here, check if user is logged in, if not, prompt that directly! 
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == 1)
                return "Welcome to the SubmissionsBot! Pick an action either by clicking the menu button or typing '/'.";
            
            if (inputMessage.Details.RecipientBotType is BotType.Submissions &&
                inputMessage.Details.BotCommandEnumCode.IsSome)
            {
                var botCommand = botCommandMenus.SubmissionsBotCommandMenu
                    .FirstOrDefault(kvp => 
                        (int)kvp.Key == inputMessage.Details.BotCommandEnumCode.GetValueOrDefault())
                    .Value.Command;

                return $"Echo of a Submissions BotCommand: {botCommand}";
            }

            return inputMessage.Details.AttachmentType.Match(
                type => $"Echo from bot Submissions: {type}",
                () => $"Echo from bot Submissions: {inputMessage.Details.Text.GetValueOrDefault()}");
        });
    }
}
