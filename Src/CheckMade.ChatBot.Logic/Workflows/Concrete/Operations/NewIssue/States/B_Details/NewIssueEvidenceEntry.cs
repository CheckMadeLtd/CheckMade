using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.B_Details;

internal interface INewIssueEvidenceEntry<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewIssueEvidenceEntry<T>(
        IDomainGlossary Glossary,
        IStateMediator Mediator) 
    : INewIssueEvidenceEntry<T> where T : ITrade, new()
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
                Text = Ui("Please (optionally) provide description and/or photos of the issue."),
                ControlPromptsSelection = ControlPrompts.Skip | ControlPrompts.Back,
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
        var promptTransitionAfterEvidenceEntry = new PromptTransition(
            currentInput.TlgMessageId, true);
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (currentInput.InputType)
        {
            case TlgInputType.TextMessage:

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
            
            case TlgInputType.AttachmentMessage:

                var currentAttachmentType = 
                    currentInput.Details.AttachmentType.GetValueOrThrow(); 
                
                return currentAttachmentType switch
                {
                    TlgAttachmentType.Photo => WorkflowResponse.Create(
                        currentInput,
                        new OutputDto
                        {
                            Text = Ui("✅📷 Photo received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    TlgAttachmentType.Document => WorkflowResponse.Create(
                        currentInput,
                        new OutputDto
                        {
                            Text = Ui("✅📄 Document received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, 
                        newState: this,
                        promptTransition: promptTransitionAfterEvidenceEntry),

                    TlgAttachmentType.Voice => WorkflowResponse.Create(
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
                            Mediator.Next(typeof(INewIssueReview<T>)),
                            new PromptTransition(currentInput.TlgMessageId)),
            
                    (long)ControlPrompts.Back => 
                        await WorkflowResponse.CreateFromNextStateAsync(
                            currentInput, 
                            Mediator.Next(typeof(INewIssueTypeSelection<T>)),
                            new PromptTransition(true)
                        ),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControl}'")
                };
            }
        }
    }
}