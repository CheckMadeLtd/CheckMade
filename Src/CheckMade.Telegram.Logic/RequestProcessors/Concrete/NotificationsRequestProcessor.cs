using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.Concrete;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor(ITelegramUpdateRepository updateRepo) : INotificationsRequestProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessRequestAsync(Result<TelegramUpdate> telegramUpdate)
    {
        if (telegramUpdate.IsSuccess)
        {
            await updateRepo.AddOrThrowAsync(telegramUpdate.Value!);
            
            if (telegramUpdate.Value!.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return new List<OutputDto>
                {
                    OutputDto.Create(UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Notifications),
                        IRequestProcessor.SeeValidBotCommandsInstruction))
                };
            }
        }
        
        return new List<OutputDto>();
    }
}