using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Telegram.Logic;

public interface IInputProcessorFactory
{
    IInputProcessor GetInputProcessor(InteractionMode interactionMode);
}

public class InputProcessorFactory(
        ITlgInputRepository inputRepo, 
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IInputProcessorFactory
{
    public IInputProcessor GetInputProcessor(InteractionMode interactionMode)
    {
        return new InputProcessor(interactionMode, inputRepo, portToRoleMapRepo);
    }
}