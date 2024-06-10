using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Telegram.Logic;

public interface IInputProcessorFactory
{
    IInputProcessor GetInputProcessor(InteractionMode interactionMode);
}

public class InputProcessorFactory(
        ITlgInputRepository inputRepo, 
        IRoleRepository roleRepo, 
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IInputProcessorFactory
{
    public IInputProcessor GetInputProcessor(InteractionMode interactionMode)
    {
        return new InputProcessor(interactionMode, inputRepo, roleRepo, portToRoleMapRepo);
    }
}