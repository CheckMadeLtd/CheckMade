using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;

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