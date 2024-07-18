using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueCleanlinessFacilitySelection : IWorkflowState; 

internal record NewIssueCleanlinessFacilitySelection(IDomainGlossary Glossary) 
    : INewIssueCleanlinessFacilitySelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}