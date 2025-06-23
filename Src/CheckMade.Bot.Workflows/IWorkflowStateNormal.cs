using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows;

public interface IWorkflowStateNormal : IWorkflowState
{
    IStateMediator Mediator { get; }
    
    Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer);

    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput);
}