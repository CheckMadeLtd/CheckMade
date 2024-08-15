using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewAssessment.States;

internal interface INewAssessmentRate : IWorkflowStateNormal;

internal sealed record NewAssessmentRate(
    IDomainGlossary Glossary, 
    IStateMediator Mediator) 
    : INewAssessmentRate
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId, 
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new OutputDto
            {
                Text = Ui("Provide cleanliness rating:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(AssessmentRating))
                        .OrderBy(dt => dt.EnumValue).ToImmutableReadOnlyCollection()),
                ControlPromptsSelection = ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableReadOnlyCollection(),
                () => outputs.ToImmutableReadOnlyCollection()));
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
                        Mediator.Next(typeof(INewAssessmentReview)),
                        promptTransition),
                
                AssessmentRating.Ok or AssessmentRating.Bad =>
                    await WorkflowResponse.CreateFromNextStateAsync(
                        currentInput,
                        Mediator.Next(typeof(INewAssessmentEvidenceEntry)),
                        promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedRating)}: {selectedRating}")
            };
        }
        
        return // on ControlPrompts.Back
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput,
                Mediator.Next(typeof(INewAssessmentFacilitySelection)),
                new PromptTransition(true));
    }
}