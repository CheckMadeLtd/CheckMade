using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Common.Model.Telegram.UserInteraction.BotCommands;

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