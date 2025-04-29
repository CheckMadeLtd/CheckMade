using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;

internal interface INewIssueTypeSelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewIssueTypeSelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator) 
    : INewIssueTypeSelection<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
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
                        .ToImmutableArray()),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId,
                ControlPromptsSelection = ControlPrompts.Back
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<ResultOld<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
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
                nameof(CleaningIssue<T>) or nameof(TechnicalIssue<T>) => 
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

        return // on ControlPrompts.Back 
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, 
                Mediator.Next(typeof(INewIssueSphereSelection<T>)),
                new PromptTransition(true));
    }
}