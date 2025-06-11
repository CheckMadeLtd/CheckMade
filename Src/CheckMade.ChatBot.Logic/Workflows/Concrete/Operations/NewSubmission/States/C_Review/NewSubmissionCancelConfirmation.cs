using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.C_Review;

internal interface INewSubmissionCancelConfirmation<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionCancelConfirmation<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator) 
    : INewSubmissionCancelConfirmation<T> where T : ITrade, new()
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
                Text = Ui("Are you sure you want to cancel drafting this new submission?"),
                ControlPromptsSelection = ControlPrompts.YesNo,
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

        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (long)ControlPrompts.Yes =>
                WorkflowResponse.Create(
                    currentInput,
                    new OutputDto
                    {
                        Text = Ui("Cancelled.")
                    },
                    newState: Mediator.GetTerminator(typeof(INewSubmissionCancelled<T>)),
                    promptTransition: new PromptTransition(currentInput.TlgMessageId)),
            
            (long)ControlPrompts.No =>
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewSubmissionReview<T>)),
                    new PromptTransition(true)),
            
            _ => throw new InvalidOperationException($"Unhandled choice of {nameof(ControlPrompts)}")
        };
    }
}