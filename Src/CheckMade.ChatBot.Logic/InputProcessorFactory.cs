using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Logic;

public interface IInputProcessorFactory
{
    IInputProcessor GetInputProcessor(InteractionMode interactionMode);
}

internal class InputProcessorFactory(
        IWorkflowIdentifier workflowIdentifier,    
        ITlgInputsRepository inputsRepo,
        ILogicUtils logicUtils,
        ILogger<InputProcessor> logger) 
    : IInputProcessorFactory
{
    public IInputProcessor GetInputProcessor(InteractionMode interactionMode)
    {
        return new InputProcessor(interactionMode, workflowIdentifier, inputsRepo, logicUtils, logger);
    }
}