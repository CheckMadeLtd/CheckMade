using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTypeSelection<T> : IWorkflowState where T : ITrade; 

internal sealed record NewIssueTypeSelection<T>(
        IDomainGlossary Glossary,
        IStateMediator Mediator) 
    : INewIssueTypeSelection<T> where T : ITrade, new()
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
                        Glossary
                            .GetAll(typeof(ITradeIssue<T>))
                            .ToImmutableReadOnlyCollection()),
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
                currentInput.Details.DomainTerm.GetValueOrThrow()
                    .TypeValue!.Name
                    .GetTypeNameWithoutGenericParamSuffix();
        
            return issueTypeName switch
            {
                nameof(CleanlinessIssue<T>) or nameof(TechnicalIssue<T>) => 
                    await WorkflowResponse.CreateAsync(
                        currentInput, Mediator.Next(typeof(INewIssueFacilitySelection<T>)),
                        true),
            
                nameof(ConsumablesIssue<T>) => 
                    await WorkflowResponse.CreateAsync(
                        currentInput, Mediator.Next(typeof(INewIssueConsumablesSelection<T>)),
                        true),
            
                nameof(StaffIssue<T>) => 
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