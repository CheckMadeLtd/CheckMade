using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Function;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;

public interface INewSubmissionEvidenceEntry<T> : IWorkflowStateNormal where T : ITrade, new(); 

public sealed record NewSubmissionEvidenceEntry<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionEvidenceEntry<T> where T : ITrade, new()
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
                Text = Ui("Please (optionally) provide description and/or photos for this submission."),
                ControlPromptsSelection = ControlPrompts.Skip | ControlPrompts.Back,
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
        var promptTransitionAfterEvidenceEntry = new PromptTransition(
            currentInput.MessageId, MsgIdCache, currentInput.Agent, true);
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (currentInput.InputType)
        {
            case InputType.TextMessage:

                return WorkflowResponse.Create(
                    currentInput,
                    new Output
                    {
                        Text = Ui("✅📝 Description received. You can send more text, add photos/documents " +
                                  "or continue to the next step."),
                        ControlPromptsSelection = ControlPrompts.Continue
                    }, 
                    newState: this,
                    promptTransition: promptTransitionAfterEvidenceEntry);
            
            case InputType.AttachmentMessage:

                var currentAttachmentType = 
                    currentInput.Details.AttachmentType.GetValueOrThrow(); 
                
                return currentAttachmentType switch
                {
                    AttachmentType.Photo => WorkflowResponse.Create(
                        currentInput,
                        new Output
                        {
                            Text = Ui("✅📷 Photo received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    AttachmentType.Document => WorkflowResponse.Create(
                        currentInput,
                        new Output
                        {
                            Text = Ui("✅📄 Document received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    AttachmentType.Voice => WorkflowResponse.Create(
                        currentInput,
                        new Output
                        {
                            Text = Ui("❗🎙 Voice messages are not yet supported. You can send photos/documents, " +
                                      "add a description or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.AttachmentType)}: '{currentAttachmentType}'")
                };
            
            default:
            {
                var selectedControl = 
                    currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

                return selectedControl switch
                {
                    (long)ControlPrompts.Skip or (long)ControlPrompts.Continue =>
                        await WorkflowResponse.CreateFromNextStateAsync(
                            currentInput, 
                            Mediator.Next(typeof(INewSubmissionReview<T>)),
                            new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.Agent)),
            
                    (long)ControlPrompts.Back => 
                        await WorkflowResponse.CreateFromNextStateAsync(
                            currentInput, 
                            Mediator.Next(typeof(INewSubmissionTypeSelection<T>)),
                            new PromptTransition(true)
                        ),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControl}'")
                };
            }
        }
    }
}