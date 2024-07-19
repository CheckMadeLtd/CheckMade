using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmation : IWorkflowState;

internal record NewIssueSphereConfirmation(
        ITrade Trade,
        ISphereOfAction Sphere,
        ILiveEventsRepository LiveEventRepo,
        IDomainGlossary Glossary) 
    : INewIssueSphereConfirmation
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
                    EditReplyMarkupOfMessageId = editMessageId
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
            (int)ControlPrompts.Yes => Trade switch
            {
                SaniCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary)),
                SiteCleanTrade =>
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary)),
                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(Trade)}: '{Trade.GetType()}'")
            },
            
            (int)ControlPrompts.No => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueSphereSelection(
                        Trade,
                        liveEventInfo,
                        LiveEventRepo,
                        Glossary)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}