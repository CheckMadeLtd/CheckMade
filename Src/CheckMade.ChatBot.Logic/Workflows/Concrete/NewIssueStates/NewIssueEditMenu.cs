using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueEditMenu<T> : IWorkflowState where T : ITrade;

internal sealed record NewIssueEditMenu<T>(IDomainGlossary Glossary) : INewIssueEditMenu<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<int> inPlaceUpdateMessageId, 
        Option<OutputDto> previousPromptFinalizer)
    {
        // Show a menu of editable items (evidence, facility, etc.) based on what's part of the history
        // i.e. not always all options
         
        
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        // once I choose one, depending on which it is, it just loops back to that state.
        // however, in each state, when I got there via edit, after submission of new input there, it 
        // needs to go straight to review i.e. it needs to check whether there is an 'edit click' input
        // since the last review in the interactive history.
        // The logic to determine this shall be in the GeneralWorkflowUtils.  
        
        throw new NotImplementedException();
    }
}