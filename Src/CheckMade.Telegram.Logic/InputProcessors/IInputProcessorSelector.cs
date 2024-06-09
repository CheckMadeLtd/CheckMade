using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Telegram.Logic.InputProcessors.Concrete;

namespace CheckMade.Telegram.Logic.InputProcessors;

public interface IInputProcessorSelector
{
    IInputProcessor GetInputProcessor(InteractionMode interactionMode);
}

public class InputProcessorSelector(
        IOperationsInputProcessor operationsProcessor,
        ICommunicationsInputProcessor communicationsProcessor,
        INotificationsInputProcessor notificationsProcessor) 
    : IInputProcessorSelector
{
    public IInputProcessor GetInputProcessor(InteractionMode interactionMode)
    {
        return interactionMode switch
        {
            InteractionMode.Operations => operationsProcessor,
            InteractionMode.Communications => communicationsProcessor,
            InteractionMode.Notifications => notificationsProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };
    }
}