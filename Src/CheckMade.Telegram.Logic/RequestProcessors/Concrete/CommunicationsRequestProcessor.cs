﻿using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
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
        // ToDo: Get rid of this try/catch
        try
        {
            await updateRepo.AddOrThrowAsync(telegramUpdate);
        }
        catch (Exception ex)
        {
            return Attempt<IReadOnlyList<OutputDto>>.Fail(ex);
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