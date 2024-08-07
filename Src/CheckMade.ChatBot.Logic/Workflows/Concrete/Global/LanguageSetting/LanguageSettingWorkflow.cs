using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;

internal interface ILanguageSettingWorkflow : IWorkflow;

internal sealed record LanguageSettingWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator), ILanguageSettingWorkflow
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput,
            Mediator.Next(typeof(ILanguageSettingSelect)));
    }
}