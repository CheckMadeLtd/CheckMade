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
        TlgInput currentInput, 
        Option<int> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("Please select the type of issue:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary
                        .GetAll(typeof(ITradeIssue<T>))
                        .ToImmutableReadOnlyCollection()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId,
                ControlPromptsSelection = ControlPrompts.Back
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>( 
            previousPromptFinalizer.Match(
                ppf =>
                {
                    outputs.Add(ppf);
                    return outputs;
                },
                () => outputs));
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

            var promptTransition =
                new PromptTransition(
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                            UiNoTranslate(" "),
                            Glossary.GetUi(currentInput.Details.DomainTerm.GetValueOrThrow())),
                        UpdateExistingOutputMessageId = currentInput.TlgMessageId
                    });
            
            return issueTypeName switch
            {
                nameof(CleanlinessIssue<T>) or nameof(TechnicalIssue<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewIssueFacilitySelection<T>)),
                        promptTransition),
            
                nameof(ConsumablesIssue<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewIssueConsumablesSelection<T>)),
                        promptTransition),
            
                nameof(StaffIssue<T>) or nameof(GeneralIssue<T>) => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput, 
                        Mediator.Next(typeof(INewIssueEvidenceEntry<T>)),
                        promptTransition),
                
                _ => throw new InvalidOperationException(
                    $"Unhandled {nameof(currentInput.Details.DomainTerm)}: '{issueTypeName}'")
            };
        }

        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput, 
            Mediator.Next(typeof(INewIssueSphereSelection<T>)),
            new PromptTransition(true));
    }
}