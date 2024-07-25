using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionConfirmation<T> : IWorkflowState;

internal sealed record NewIssueSubmissionConfirmation<T>(IDomainGlossary Glossary) : INewIssueSubmissionConfirmation<T>
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(TlgInput currentInput, Option<int> editMessageId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
};