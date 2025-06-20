using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Workflows.Global.UserAuth.States;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows.Global.UserAuth;

public sealed record UserAuthWorkflow(
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
            Mediator.Next(typeof(IUserAuthWorkflowTokenEntry)));
    }
}