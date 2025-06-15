using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.DomainModel.Data.ChatBot;
using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.Output;
using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;
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