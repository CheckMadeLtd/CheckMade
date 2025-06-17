using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
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