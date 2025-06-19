using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Bot;
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