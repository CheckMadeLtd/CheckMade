using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Utils.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTradeSelection : IWorkflowState;

internal sealed record NewIssueTradeSelection(
        IDomainGlossary Glossary,
        ILiveEventsRepository LiveEventRepo,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator)
    : INewIssueTradeSelection
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
                Text = Ui("Please select a Trade:"),
                DomainTermSelection = 
                    Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITrade))),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf =>
                {
                    outputs.Add(ppf);
                    return outputs;
                },
                () => outputs));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var selectedTradeDt = currentInput.Details.DomainTerm.GetValueOrThrow(); 
        var selectedTrade = (ITrade)Activator.CreateInstance(selectedTradeDt.TypeValue!)!; 
        
        var promptTransition =
            new PromptTransition(
                new OutputDto
                {
                    Text = UiConcatenate(
                        UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                        UiNoTranslate(" "),
                        Glossary.GetUi(selectedTradeDt)),
                    UpdateExistingOutputMessageId = currentInput.TlgMessageId
                });
        
        return await (await GetSphereNearUserAsync()).Match(
            _ => selectedTrade switch 
            { 
                SaniCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereConfirmation<SaniCleanTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereConfirmation<SiteCleanTrade>)),
                    promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            }, 
            () => selectedTrade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereSelection<SaniCleanTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>)),
                    promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            });

        async Task<Option<ISphereOfAction>> GetSphereNearUserAsync()
        {
            var lastKnownLocation = 
                await LastKnownLocationAsync(currentInput, GeneralWorkflowUtils);

            var liveEvent =
                (await LiveEventRepo.GetAsync(
                    currentInput.LiveEventContext.GetValueOrThrow()))!;
        
            return lastKnownLocation.IsSome
                ? SphereNearCurrentUser(
                    liveEvent, lastKnownLocation.GetValueOrThrow(), selectedTrade)
                : Option<ISphereOfAction>.None();
        }
    }
}