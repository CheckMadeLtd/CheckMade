using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTypeSelection<T> : IWorkflowState where T : ITrade; 

internal record NewIssueTypeSelection<T>(
        IDomainGlossary Glossary,
        IStateMediator Mediator) 
    : INewIssueTypeSelection<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please select the type of issue:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITradeIssue))),
                    EditPreviousOutputMessageId = editMessageId,
                    ControlPromptsSelection = ControlPrompts.Back
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var issueTypeName = 
                currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!.Name;
        
            return issueTypeName switch
            {
                nameof(CleanlinessIssue) => 
                    await WorkflowResponse.CreateAsync(
                        currentInput, Mediator.Next(typeof(INewIssueFacilitySelection<T>)),
                        true),
            
                nameof(ConsumablesIssue) => 
                    await WorkflowResponse.CreateAsync(
                        currentInput, Mediator.Next(typeof(INewIssueConsumablesSelection<T>)),
                        true),
            
                nameof(TechnicalIssue) or nameof(StaffIssue) => 
                    await WorkflowResponse.CreateAsync(
                        currentInput, Mediator.Next(typeof(INewIssueEvidenceEntry<T>)),
                        true),
            
                _ => throw new InvalidOperationException(
                    $"Unhandled {nameof(currentInput.Details.DomainTerm)}: '{issueTypeName}'")
            };
        }

        return await WorkflowResponse.CreateAsync(
            currentInput, Mediator.Next(typeof(INewIssueSphereSelection<T>)));
    }
}