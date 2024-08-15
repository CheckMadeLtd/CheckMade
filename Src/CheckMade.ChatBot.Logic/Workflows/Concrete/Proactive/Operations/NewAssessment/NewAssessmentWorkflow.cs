using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewAssessment;

internal sealed record NewAssessmentWorkflow(
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary)
    : WorkflowBase(WorkflowUtils, Mediator, BridgesRepo, Glossary)
{
    protected override Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}