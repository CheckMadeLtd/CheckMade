using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.ByBotType;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor(IMessageRepository repo) : INotificationsRequestProcessor
{
    public async Task<Attempt<OutputDto>> ProcessRequestAsync(InputMessageDto inputMessage)
    {
        try
        {
            await repo.AddOrThrowAsync(inputMessage);
        }
        catch (Exception ex)
        {
            return Attempt<OutputDto>.Fail(new Error(Exception: ex));
        }
        
        return await Attempt<OutputDto>.RunAsync(() =>
        {
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return Task.FromResult(OutputDto.Create(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Notifications),
                        IRequestProcessor.SeeValidBotCommandsInstruction)));
            }

            return Task.FromResult(OutputDto.CreateEmpty());
        });
    }
}