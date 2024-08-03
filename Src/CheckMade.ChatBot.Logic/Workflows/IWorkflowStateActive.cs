using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowStateActive : IWorkflowState
{
    Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<int> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer);
    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput);
    IDomainGlossary Glossary { get; }
}