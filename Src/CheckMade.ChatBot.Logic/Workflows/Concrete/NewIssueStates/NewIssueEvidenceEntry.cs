using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueEvidenceEntry<T> : IWorkflowState where T : ITrade; 

internal sealed record NewIssueEvidenceEntry<T>(
        IDomainGlossary Glossary,
        IStateMediator Mediator) 
    : INewIssueEvidenceEntry<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please (optionally) provide description and/or photos of the issue."),
                    ControlPromptsSelection = ControlPrompts.Skip | ControlPrompts.Back,
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (currentInput.InputType)
        {
            case TlgInputType.TextMessage:
                
                return new WorkflowResponse(
                    new OutputDto
                    {
                        Text = Ui("✅📝 Description received. You can send more text, add photos/documents " + 
                                  "or continue to the next step."),
                        ControlPromptsSelection = ControlPrompts.Continue
                    }, Glossary.GetId(GetType().GetInterfaces()[0]));
            
            case TlgInputType.AttachmentMessage:

                var currentAttachmentType = 
                    currentInput.Details.AttachmentType.GetValueOrThrow(); 
                
                return currentAttachmentType switch
                {
                    TlgAttachmentType.Photo => new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("✅📷 Photo received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, Glossary.GetId(GetType().GetInterfaces()[0])),

                    TlgAttachmentType.Document => new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("✅📄 Document received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, Glossary.GetId(GetType().GetInterfaces()[0])),

                    TlgAttachmentType.Voice => new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("❗🎙 Voice messages are not yet supported. You can send photos/documents, " +
                                      "add a description or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, Glossary.GetId(GetType().GetInterfaces()[0])),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.AttachmentType)}: '{currentAttachmentType}'")
                };
            
            default:
            {
                var selectedControlPrompt = 
                    currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

                return selectedControlPrompt switch
                {
                    (long)ControlPrompts.Skip or (long)ControlPrompts.Continue =>
                        await WorkflowResponse.CreateAsync(
                            currentInput, Mediator.Next(typeof(INewIssueReview<T>))),
            
                    (long)ControlPrompts.Back => 
                        await WorkflowResponse.CreateAsync(
                            currentInput, Mediator.Next(typeof(INewIssueTypeSelection<T>)), 
                            true),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
                };
            }
        }
    }
}