using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

public interface INotificationsUpdateProcessor : IUpdateProcessor;

public class NotificationsUpdateProcessor(ITelegramUpdateRepository updateRepo) : INotificationsUpdateProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessUpdateAsync(Result<TelegramUpdate> telegramUpdate)
    {
        return await telegramUpdate.Match(
            async successfulUpdate =>
            {
                await updateRepo.AddAsync(successfulUpdate);

                if (successfulUpdate.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
                {
                    return new List<OutputDto>
                    {
                        new()
                        {
                            Text = UiConcatenate(
                                Ui("Welcome to the CheckMade {0} Bot! ", BotType.Notifications), 
                                IUpdateProcessor.SeeValidBotCommandsInstruction) 
                        }
                    };
                }
                
                return new[] { new OutputDto() };
            },

            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }
}