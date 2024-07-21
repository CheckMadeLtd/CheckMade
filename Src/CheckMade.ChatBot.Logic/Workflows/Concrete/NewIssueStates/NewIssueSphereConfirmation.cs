using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmation<T> : IWorkflowState where T : ITrade;

internal record NewIssueSphereConfirmation<T>(
        ISphereOfAction Sphere,
        ILiveEventsRepository LiveEventRepo,
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils) 
    : INewIssueSphereConfirmation<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto> 
            {
                new()
                {
                    Text = Ui("Please confirm: are you at '{0}'?", Sphere.Name),
                    ControlPromptsSelection = ControlPrompts.YesNo,
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var liveEventInfo = 
            currentInput.LiveEventContext.GetValueOrThrow();
        
        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (int)ControlPrompts.Yes => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueTypeSelection<T>(Glossary, LogicUtils)),
            
            (int)ControlPrompts.No => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueSphereSelection<T>(
                        liveEventInfo, LiveEventRepo, Glossary, LogicUtils)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}