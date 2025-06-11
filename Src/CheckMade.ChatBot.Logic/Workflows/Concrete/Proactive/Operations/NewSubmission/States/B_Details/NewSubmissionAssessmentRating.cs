using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Submissions.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionAssessmentRating<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionAssessmentRating<T>(
    IDomainGlossary Glossary, 
    IStateMediator Mediator) 
    : INewSubmissionAssessmentRating<T> where T : ITrade, new()
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
                Text = Ui("Provide rating:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(AssessmentRating))
                        .OrderBy(static dt => dt.EnumValue).ToImmutableArray()),
                ControlPromptsSelection = ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType != TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var selectedRating = currentInput.Details.DomainTerm.GetValueOrThrow().EnumValue!;

            var promptTransition = new PromptTransition(
                new OutputDto
                {
                    Text = UiConcatenate(
                        UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                        UiNoTranslate(" "),
                        Glossary.GetUi(selectedRating)),
                    UpdateExistingOutputMessageId = currentInput.TlgMessageId
                });
            
            return selectedRating switch
            {
                AssessmentRating.Good => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput,
                        Mediator.Next(typeof(INewIssueReview<T>)),
                        new PromptTransition(currentInput.TlgMessageId)),
                
                AssessmentRating.Ok or AssessmentRating.Bad =>
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput,
                        Mediator.Next(typeof(INewSubmissionEvidenceEntry<T>)),
                        promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedRating)}: {selectedRating}")
            };
        }
        
        return // on ControlPrompts.Back
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput,
                Mediator.Next(typeof(INewSubmissionFacilitySelection<T>)),
                new PromptTransition(true));
    }
}