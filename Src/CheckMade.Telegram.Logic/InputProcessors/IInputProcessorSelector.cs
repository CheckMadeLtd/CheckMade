using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Telegram.Logic.InputProcessors.Concrete;

namespace CheckMade.Telegram.Logic.InputProcessors;

public interface IInputProcessorSelector
{
    IInputProcessor GetInputProcessor(TlgBotType botType);
}

public class InputProcessorSelector(
        IOperationsInputProcessor operationsProcessor,
        ICommunicationsInputProcessor communicationsProcessor,
        INotificationsInputProcessor notificationsProcessor) 
    : IInputProcessorSelector
{
    public IInputProcessor GetInputProcessor(TlgBotType botType)
    {
        return botType switch
        {
            TlgBotType.Operations => operationsProcessor,
            TlgBotType.Communications => communicationsProcessor,
            TlgBotType.Notifications => notificationsProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };
    }
}