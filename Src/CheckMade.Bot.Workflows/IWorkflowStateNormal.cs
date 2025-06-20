using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Utils;
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