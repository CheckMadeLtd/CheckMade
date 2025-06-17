using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowStateNormal : IWorkflowState
{
    IStateMediator Mediator { get; }
    
    Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer);

    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput);
}