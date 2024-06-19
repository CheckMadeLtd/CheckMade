using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    Task<LanguageSettingWorkflow.States> DetermineCurrentStateAsync(TlgClientPort clientPort);
}

internal class LanguageSettingWorkflow(
        ITlgClientPortRoleRepository portRoleRepo,
        IWorkflowUtils workflowUtils) 
    : ILanguageSettingWorkflow
{
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        throw new NotImplementedException();
    }

    public async Task<States> DetermineCurrentStateAsync(TlgClientPort clientPort)
    {
        var allCurrentInputs = await workflowUtils.GetAllCurrentInputsAsync(clientPort);
        var lastInput = allCurrentInputs[^1];

        return lastInput.InputType switch
        {
            TlgInputType.CommandMessage => States.Initial,
            TlgInputType.CallbackQuery => States.ReceivedLanguageSetting,
            _ => States.Initial
        };
    }
    
    [Flags]
    internal enum States
    {
        Initial = 1,
        ReceivedLanguageSetting = 1<<1
    }
}