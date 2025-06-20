using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Workflows.Global.Logout.States;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows.Global.Logout;

public sealed record LogoutWorkflow(
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
            Mediator.Next(typeof(ILogoutWorkflowConfirm)));
    }
}