using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;

public interface INewSubmissionEditMenu<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionEditMenu<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IGeneralWorkflowUtils GeneralUtils) 
    : INewSubmissionEditMenu<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId, 
        Option<Output> previousPromptFinalizer)
    {
        var workflowStateHistory =
            (await GeneralUtils.GetInteractiveWorkflowHistoryAsync(currentInput))
            .Select(static i => i.ResultantState)
            .Where(static rw => rw.IsSome)
            .Select(static rw => rw.GetValueOrThrow().InStateId)
            .ToArray();

        List<DomainTerm> editMenu = [];

        if (workflowStateHistory
            .Contains(Glossary.GetId(typeof(INewSubmissionConsumablesSelection<T>))))
        {
            editMenu.Add(Dt(typeof(INewSubmissionConsumablesSelection<T>)));
        }
        
        // ReSharper disable once UnusedVariable
        List<Output> outputs = 
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

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
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