using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueEvidenceEntry : IWorkflowState; 

internal record NewIssueEvidenceEntry<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils) 
    : INewIssueEvidenceEntry where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
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
                    }, Glossary.GetId(GetType()));
            
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
                        }, Glossary.GetId(GetType())),

                    TlgAttachmentType.Document => new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("✅📄 Document received. You can send more attachments, add a description " +
                                      "or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, Glossary.GetId(GetType())),

                    TlgAttachmentType.Voice => new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("❗🎙 Voice messages are not yet supported. You can send photos/documents, " +
                                      "add a description or continue to the next step."),
                            ControlPromptsSelection = ControlPrompts.Continue
                        }, Glossary.GetId(GetType())),
                    
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
                        await WorkflowResponse.CreateAsync(new NewIssueReview<T>(Glossary)),
            
                    (long)ControlPrompts.Back => 
                        await LogicUtils.GetPreviousStateNameAsync(currentInput, -2) switch
                            {
                                nameof(NewIssueTypeSelection<ITrade>) =>
                                    await WorkflowResponse.CreateAsync(
                                        new NewIssueTypeSelection<T>(Glossary, LogicUtils),
                                        currentInput.Details.TlgMessageId),
                                
                                nameof(NewIssueFacilitySelection<ITrade>) =>
                                    await WorkflowResponse.CreateAsync(
                                        new NewIssueFacilitySelection<T>(Glossary, LogicUtils),
                                        currentInput.Details.TlgMessageId),
                                
                                _ => throw new InvalidOperationException(
                                    $"Unhandled {nameof(LogicUtils.GetPreviousStateNameAsync)}: " +
                                    $"'{await LogicUtils.GetPreviousStateNameAsync(currentInput, -2)}'")
                            },
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
                };
            }
        }
    }
}