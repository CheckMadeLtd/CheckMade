using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.DomainModel.ChatBot;
using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.Output;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowStateNormal : IWorkflowState
{
    IStateMediator Mediator { get; }
    
    Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer);

    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput);
}