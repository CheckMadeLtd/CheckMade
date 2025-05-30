using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.C_Review;

internal interface INewIssueEditMenu<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewIssueEditMenu<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IGeneralWorkflowUtils GeneralUtils) 
    : INewIssueEditMenu<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<TlgMessageId> inPlaceUpdateMessageId, 
        Option<OutputDto> previousPromptFinalizer)
    {
        var workflowStateHistory =
            (await GeneralUtils.GetInteractiveWorkflowHistoryAsync(currentInput))
            .Select(static i => i.ResultantWorkflow)
            .Where(static rw => rw.IsSome)
            .Select(static rw => rw.GetValueOrThrow().InStateId)
            .ToArray();

        List<DomainTerm> editMenu = [];

        if (workflowStateHistory
            .Contains(Glossary.GetId(typeof(INewIssueConsumablesSelection<T>))))
        {
            editMenu.Add(Dt(typeof(INewIssueConsumablesSelection<T>)));
        }
        
        // ReSharper disable once UnusedVariable
        List<OutputDto> outputs = 
        [
            new()
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