using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.Submissions;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;

public interface INewSubmissionAssessmentRating<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionAssessmentRating<T>(
    IDomainGlossary Glossary, 
    IStateMediator Mediator,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionAssessmentRating<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId, 
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
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
        
        return Task.FromResult<IReadOnlyCollection<Output>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType != InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var selectedRating = currentInput.Details.DomainTerm.GetValueOrThrow().EnumValue!;

            var promptTransition = new PromptTransition(
                new Output
                {
                    Text = UiConcatenate(
                        UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                        UiNoTranslate(" "),
                        Glossary.GetUi(selectedRating)),
                    UpdateExistingOutputMessageId = currentInput.MessageId
                });
            
            return selectedRating switch
            {
                AssessmentRating.Good => 
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput,
                        Mediator.Next(typeof(INewSubmissionReview<T>)),
                        new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.Agent)),
                
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