using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssue.States;

internal interface INewIssueEditMenu<T> : IWorkflowStateNormal where T : ITrade;

internal sealed record NewIssueEditMenu<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralUtils) 
    : INewIssueEditMenu<T> where T : ITrade
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<int> inPlaceUpdateMessageId, 
        Option<OutputDto> previousPromptFinalizer)
    {
        var workflowStateHistory =
            (await GeneralUtils.GetInteractiveSinceLastBotCommandAsync(currentInput))
            .Select(i => i.ResultantWorkflow)
            .Where(rw => rw.IsSome)
            .Select(rw => rw.GetValueOrThrow().InStateId)
            .ToImmutableReadOnlyCollection();

        List<DomainTerm> editMenu = [];

        if (workflowStateHistory
            .Contains(Glossary.GetId(typeof(INewIssueConsumablesSelection<T>))))
        {
            editMenu.Add(Dt(typeof(INewIssueConsumablesSelection<T>)));
        }
        
        // ReSharper disable once UnusedVariable
        List<OutputDto> outputs = 
        [
            new OutputDto
            {
                Text = Ui("Choose item to edit:"), 
                DomainTermSelection = editMenu,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        // once I choose one, depending on which it is, it just loops back to that state.
        // however, in each state, when I got there via edit, after submission of new input there, it 
        // needs to go straight to review i.e. it needs to check whether there is an 'edit click' input
        // since the last review in the interactive history.
        // The logic to determine this shall be in the GeneralWorkflowUtils.  
        
        // But there is potential complications if what the user changed requires new further input.
        // Delay to post MVP.
        
        throw new NotImplementedException();
    }
}