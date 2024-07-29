using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionConfirmation<T> : IWorkflowState;

internal sealed record NewIssueSubmissionConfirmation<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils GeneralWorkflowUtils,
    ITlgInputsRepository InputsRepo,
    IIssueFactory<T> Factory) 
    : INewIssueSubmissionConfirmation<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(TlgInput currentInput, Option<int> editMessageId)
    {
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        var lastGuid = interactiveHistory
            .Select(i => i.EntityGuid)
            .Last(g => g.IsSome)
            .GetValueOrThrow();
        var updatedHistoryWithGuid = 
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(
                currentInput with
                {
                    EntityGuid = lastGuid
                });
        
        var issue = await Factory.CreateAsync(updatedHistoryWithGuid);
        var summary = issue.GetSummary();
        
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Success!")
            },
            new()
            {
                Text = UiConcatenate(
                    summary.Where(kvp => 
                        (IssueSummaryCategories.All & kvp.Key) != 0)
                        .Select(kvp => kvp.Value)
                        .ToArray()),
                LogicalPort = new LogicalPort()
            }
        };
        
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
};