using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;

internal sealed record LanguageSettingWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput,
            Mediator.Next(typeof(ILanguageSettingSelect)));
    }
}