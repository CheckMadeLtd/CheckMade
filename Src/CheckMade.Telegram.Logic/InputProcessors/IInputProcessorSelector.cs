using CheckMade.Common.Model.Tlg;
using CheckMade.Telegram.Logic.InputProcessors.Concrete;

namespace CheckMade.Telegram.Logic.InputProcessors;

public interface IInputProcessorSelector
{
    IInputProcessor GetInputProcessor(TlgInteractionMode interactionMode);
}

public class InputProcessorSelector(
        IOperationsInputProcessor operationsProcessor,
        ICommunicationsInputProcessor communicationsProcessor,
        INotificationsInputProcessor notificationsProcessor) 
    : IInputProcessorSelector
{
    public IInputProcessor GetInputProcessor(TlgInteractionMode interactionMode)
    {
        return interactionMode switch
        {
            TlgInteractionMode.Operations => operationsProcessor,
            TlgInteractionMode.Communications => communicationsProcessor,
            TlgInteractionMode.Notifications => notificationsProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };
    }
}