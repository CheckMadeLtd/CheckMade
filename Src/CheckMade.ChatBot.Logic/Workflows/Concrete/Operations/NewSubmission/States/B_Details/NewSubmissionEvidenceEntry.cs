using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.B_Details;

internal interface INewSubmissionEvidenceEntry<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewSubmissionEvidenceEntry<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionEvidenceEntry<T> where T : ITrade, new()
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("Please (optionally) provide description and/or photos for this submission."),
                ControlPromptsSelection = ControlPrompts.Skip | ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
    
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
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
                    new OutputDto
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
                        new OutputDto
                        {
                            Text = Ui("✅📷 Photo received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    AttachmentType.Document => WorkflowResponse.Create(
                        currentInput,
                        new OutputDto
                        {
                            Text = Ui("✅📄 Document received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    AttachmentType.Voice => WorkflowResponse.Create(
                        currentInput,
                        new OutputDto
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