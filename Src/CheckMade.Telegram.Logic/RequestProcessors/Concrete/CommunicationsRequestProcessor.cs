using CheckMade.Common.Interfaces;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.TelegramUpdates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.Concrete;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor(IMessageRepository repo) : ICommunicationsRequestProcessor
{
    public async Task<Attempt<IReadOnlyList<OutputDto>>> ProcessRequestAsync(TelegramUpdateDto telegramUpdate)
    {
        try
        {
            await repo.AddOrThrowAsync(telegramUpdate);
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
                    OutputDto.Create(
                        new OutputDestination(BotType.Communications, 
                            new Role("token", RoleType.SanitaryOps_Admin)),
                        UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Communications), 
                        IRequestProcessor.SeeValidBotCommandsInstruction))
                };
            }

            return new List<OutputDto>();
        });
    }
}