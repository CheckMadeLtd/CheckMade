using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

public interface ICommunicationsUpdateProcessor : IUpdateProcessor; 

public class CommunicationsUpdateProcessor(ITelegramUpdateRepository updateRepo) : ICommunicationsUpdateProcessor
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
                        OutputDto.Create(UiConcatenate(
                            Ui("Welcome to the CheckMade {0} Bot! ", BotType.Communications), 
                            IUpdateProcessor.SeeValidBotCommandsInstruction))
                    };
                }
                
                return new[] { OutputDto.Create(Ui()) };
            },

            error => Task.FromResult<IReadOnlyList<OutputDto>>([ OutputDto.Create(error) ])
        );
    }
}