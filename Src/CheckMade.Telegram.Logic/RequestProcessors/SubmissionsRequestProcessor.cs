using CheckMade.Common.LangExt;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<Attempt<UiString>> SafelyEchoAsync(InputMessageDto inputMessage)
    {
        return await Attempt<UiString>.RunAsync(async () =>
        {
            await repo.AddOrThrowAsync(inputMessage);

            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
                return UiConcatenate(
                    Ui("Welcome to the CheckMade {0}Bot! ", BotType.Submissions), 
                    IRequestProcessor.SeeValidBotCommandsInstruction);
            
            if (inputMessage.BotType is BotType.Submissions &&
                inputMessage.Details.BotCommandEnumCode.IsSome)
            {
                return UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Submissions),
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString()));
            }

            return inputMessage.Details.AttachmentType.Match(
                type => Ui("Echo from bot {0}: {1}", 
                    BotType.Submissions, type),
                () => Ui("Echo from bot {0}: {1}", 
                    BotType.Submissions, inputMessage.Details.Text.GetValueOrDefault()));
        });
    }
}
