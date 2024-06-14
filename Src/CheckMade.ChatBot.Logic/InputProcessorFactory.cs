using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessorFactory
{
    IInputProcessor GetInputProcessor(InteractionMode interactionMode);
}

public class InputProcessorFactory(
        IWorkflowIdentifier workflowIdentifier,    
        ITlgInputRepository inputRepo) 
    : IInputProcessorFactory
{
    public IInputProcessor GetInputProcessor(InteractionMode interactionMode)
    {
        return new InputProcessor(interactionMode, workflowIdentifier, inputRepo);
    }
}