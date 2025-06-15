using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Submissions;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionAssessmentRating<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionAssessmentRating<T>(
    IDomainGlossary Glossary, 
    IStateMediator Mediator,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionAssessmentRating<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<MessageId> inPlaceUpdateMessageId, 
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
        if (currentInput.InputType != InputType.CallbackQuery)
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