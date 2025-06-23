using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Workflows.Global.LanguageSetting.States;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows.Global.LanguageSetting;

public sealed record LanguageSettingWorkflow(
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary) 
    : WorkflowBase(WorkflowUtils, Mediator, BridgesRepo, Glossary)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(Input currentInput)
    {
        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput,
            Mediator.Next(typeof(ILanguageSettingSelect)));
    }
}