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

            // ToDo: here, check if user is logged in, if not, prompt that directly! 
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == 1)
                return Ui(
                    "Willkommen zum SubmissionsBot! Klick auf den Menüknopf oder tippe '/' " +
                    "um verfügbare Befehle zu sehen.");
            
            if (inputMessage.Details.RecipientBotType is BotType.Submissions &&
                inputMessage.Details.BotCommandEnumCode.IsSome)
            {
                var botCommand = botCommandMenus.SubmissionsBotCommandMenu
                    .FirstOrDefault(kvp => 
                        (int)kvp.Key == inputMessage.Details.BotCommandEnumCode.GetValueOrDefault())
                    .Value.Command;

                return Ui($"Echo of a Submissions BotCommand: {botCommand}");
            }

            return inputMessage.Details.AttachmentType.Match(
                type => Ui($"Echo from bot Submissions: {type}"),
                () => Ui($"Echo from bot Submissions: {inputMessage.Details.Text.GetValueOrDefault()}"));
        });
    }
}
