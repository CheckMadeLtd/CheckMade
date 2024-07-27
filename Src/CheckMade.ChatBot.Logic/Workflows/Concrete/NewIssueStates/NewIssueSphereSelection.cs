using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Utils.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereSelection<T> : IWorkflowState where T : ITrade;

internal sealed record NewIssueSphereSelection<T> : INewIssueSphereSelection<T> where T : ITrade, new()
{
    private readonly ILiveEventsRepository _liveEventsRepo;
    private readonly IStateMediator _mediator;
    private readonly ITrade _trade;
    
    private IReadOnlyCollection<string>? _tradeSpecificSphereNamesCache;
    
    public NewIssueSphereSelection(
        ILiveEventsRepository liveEventsRepo,
        IDomainGlossary glossary,
        IStateMediator mediator)
    {
        _liveEventsRepo = liveEventsRepo;
        Glossary = glossary;
        _mediator = mediator;
        
        _trade = new T();
    }
    
    public IDomainGlossary Glossary { get; }
    
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
        
        return new List<OutputDto>
        {
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), _trade.GetSphereOfActionLabel, UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo))
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
        
        if (currentInput.InputType is not TlgInputType.TextMessage ||
            !(await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo))
                .Contains(currentInput.Details.Text.GetValueOrThrow()))
        {
            return WorkflowResponse.CreateWarningChooseReplyKeyboardOptions(
                this, await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo));
        }

        return await WorkflowResponse.CreateAsync(
            currentInput, _mediator.Next(typeof(INewIssueTypeSelection<T>)));
    }

    private async Task<IReadOnlyCollection<string>> GetTradeSpecificSphereNamesAsync(
        ITrade trade, ILiveEventInfo liveEventInfo)
    {
        return _tradeSpecificSphereNamesCache ??= 
            GetAllTradeSpecificSpheres(
                    (await _liveEventsRepo.GetAsync(liveEventInfo))!,
                    trade)
                .Select(soa => soa.Name)
                .ToImmutableReadOnlyCollection();
    }
}