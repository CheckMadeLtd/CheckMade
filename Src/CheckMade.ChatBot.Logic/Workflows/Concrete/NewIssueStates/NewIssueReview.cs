using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview<T> : IWorkflowState where T : ITrade;

internal sealed record NewIssueReview<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator,
        IIssueFactory<T> Factory) 
    : INewIssueReview<T> where T : ITrade
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        
        // ToDo: set the guid now
        
        var issue = await Factory.CreateAsync(
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput));
        
        return new List<OutputDto>
        {
            new()
            {
                Text = UiConcatenate(
                    Ui("Please review this issue draft.\n\n"),
                    issue.GetSummary()),
                ControlPromptsSelection = ControlPrompts.Submit
            }
        };
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}