using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Function;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Bot.Workflows.Workflows.Operations.NewSubmission.States.D_Terminators;
using CheckMade.Bot.Workflows.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Workflows.Operations.NewSubmission.States.C_Review;

public interface INewSubmissionCancelConfirmation<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionCancelConfirmation<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionCancelConfirmation<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId, 
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
        [
            new Output
            {
                Text = Ui("Are you sure you want to cancel drafting this new submission?"),
                ControlPromptsSelection = ControlPrompts.YesNo,
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

        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (long)ControlPrompts.Yes =>
                WorkflowResponse.Create(
                    currentInput,
                    new Output
                    {
                        Text = Ui("Cancelled.")
                    },
                    newState: Mediator.GetTerminator(typeof(INewSubmissionCancelled<T>)),
                    promptTransition: new PromptTransition(
                        currentInput.MessageId, MsgIdCache, currentInput.Agent)),
            
            (long)ControlPrompts.No =>
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewSubmissionReview<T>)),
                    new PromptTransition(true)),
            
            _ => throw new InvalidOperationException($"Unhandled choice of {nameof(ControlPrompts)}")
        };
    }
}