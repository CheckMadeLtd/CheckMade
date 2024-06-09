using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Core.Enums;
using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Model.Tlg.Output;
using CheckMade.Telegram.Model.BotCommand;

namespace CheckMade.Telegram.Logic.InputProcessors.Concrete;

public interface INotificationsInputProcessor : IInputProcessor;

public class NotificationsInputProcessor(ITlgInputRepository inputRepo) : INotificationsInputProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput)
    {
        return await tlgInput.Match(
            async input =>
            {
                await inputRepo.AddAsync(input);

                if (input.Details.BotCommandEnumCode.GetValueOrDefault() == TlgStart.CommandCode)
                {
                    return new List<OutputDto>
                    {
                        new()
                        {
                            Text = UiConcatenate(
                                Ui("Welcome to the CheckMade {0} Bot! ", InteractionMode.Notifications), 
                                IInputProcessor.SeeValidBotCommandsInstruction) 
                        }
                    };
                }
                
                return new[] { new OutputDto() };
            },

            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }
}