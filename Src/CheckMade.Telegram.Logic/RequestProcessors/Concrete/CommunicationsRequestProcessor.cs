using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.TelegramUpdates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.Concrete;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor(
        ITelegramUpdateRepository updateRepo) 
    : ICommunicationsRequestProcessor
{
    public async Task<Attempt<IReadOnlyList<OutputDto>>> ProcessRequestAsync(TelegramUpdate telegramUpdate)
    {
        try
        {
            await updateRepo.AddOrThrowAsync(telegramUpdate);
        }
        catch (Exception ex)
        {
            return Attempt<IReadOnlyList<OutputDto>>.Fail(new Error(Exception: ex));
        }

        return Attempt<IReadOnlyList<OutputDto>>.Run(() =>
        {
            if (telegramUpdate.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return new List<OutputDto>
                {
                    OutputDto.Create(UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Communications), 
                        IRequestProcessor.SeeValidBotCommandsInstruction))
                };
            }

            return new List<OutputDto>();
        });
    }
}