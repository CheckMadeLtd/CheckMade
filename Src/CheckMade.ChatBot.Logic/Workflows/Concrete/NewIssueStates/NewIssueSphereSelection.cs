using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereSelection<T> : IWorkflowState where T : ITrade;

internal record NewIssueSphereSelection<T> : INewIssueSphereSelection<T> where T : ITrade
{
    private readonly ILiveEventInfo _liveEventInfo;
    private readonly ILiveEventsRepository _liveEventsRepo;
    private readonly ILogicUtils _logicUtils;
    private readonly ITrade _trade;
    
    private IReadOnlyCollection<string>? _tradeSpecificSphereNamesCache;
    
    public NewIssueSphereSelection(
        ILiveEventInfo liveEventInfo,
        ILiveEventsRepository liveEventsRepo,
        IDomainGlossary glossary,
        ILogicUtils logicUtils)
    {
        _liveEventInfo = liveEventInfo;
        Glossary = glossary;
        _logicUtils = logicUtils;
        _liveEventsRepo = liveEventsRepo;

        _trade = (ITrade)Activator.CreateInstance(typeof(T))!;
    }
    
    public IDomainGlossary Glossary { get; }
    
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), _trade.GetSphereOfActionLabel, UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    await GetTradeSpecificSphereNamesAsync(_trade))
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.TextMessage ||
            !(await GetTradeSpecificSphereNamesAsync(_trade))
                .Contains(currentInput.Details.Text.GetValueOrThrow()))
        {
            return WorkflowResponse.CreateWarningChooseReplyKeyboardOptions(
                this, await GetTradeSpecificSphereNamesAsync(_trade));
        }

        return await WorkflowResponse.CreateAsync(
            currentInput,
            new NewIssueTypeSelection<T>(Glossary, _logicUtils));
    }

    private async Task<IReadOnlyCollection<string>> GetTradeSpecificSphereNamesAsync(ITrade trade)
    {
        return _tradeSpecificSphereNamesCache ??= 
            (await _liveEventsRepo.GetAsync(_liveEventInfo))!
            .DivIntoSpheres
            .Where(soa => soa.GetTradeType() == trade.GetType())
            .Select(soa => soa.Name)
            .ToImmutableReadOnlyCollection();
    }
}