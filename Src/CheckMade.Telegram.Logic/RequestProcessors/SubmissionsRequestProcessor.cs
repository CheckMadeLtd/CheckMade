using CheckMade.Common.LangExt.MonadicWrappers;
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

            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
                return string.Format(IRequestProcessor.WelcomeToBot, BotType.Submissions);
            
            if (inputMessage.Details.RecipientBotType is BotType.Submissions &&
                inputMessage.Details.BotCommandEnumCode.IsSome)
            {
                var botCommand = botCommandMenus.SubmissionsBotCommandMenu
                    .FirstOrDefault(kvp => 
                        (int)kvp.Key == inputMessage.Details.BotCommandEnumCode.GetValueOrDefault())
                    .Value.Command;

                return UiSm($"Echo of a Submissions BotCommand: {botCommand}");
            }

            return inputMessage.Details.AttachmentType.Match(
                type => UiSm($"Echo from bot Submissions: {type}"),
                () => UiSm($"Echo from bot Submissions: {inputMessage.Details.Text.GetValueOrDefault()}"));
        });
    }
}
