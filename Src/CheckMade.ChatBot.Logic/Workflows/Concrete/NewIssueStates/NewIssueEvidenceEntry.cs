using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueEvidenceEntry : IWorkflowState; 

internal record NewIssueEvidenceEntry(IDomainGlossary Glossary) : INewIssueEvidenceEntry
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
    {
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please (optionally) provide description and/or photos of the issue."),
                    ControlPromptsSelection = ControlPrompts.SaveSkip,
                    EditReplyMarkupOfMessageId = editMessageId
                }
            });
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}