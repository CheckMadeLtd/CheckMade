using CheckMade.Common.LangExt;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<Attempt<UiString>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return await Attempt<UiString>.RunAsync(async () =>
        {
            await repo.AddOrThrowAsync(inputMessage);

            var botCommandMenus = new BotCommandMenus();

            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
                return UiConcatenate(
                    Ui("Willkommen zum {0} Bot! ", BotType.Submissions), 
                    IRequestProcessor.WelcomeToBotMenuInstruction);
            
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
