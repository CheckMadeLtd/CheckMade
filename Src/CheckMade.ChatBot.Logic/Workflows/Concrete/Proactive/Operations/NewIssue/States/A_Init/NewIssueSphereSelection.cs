using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.NewIssueUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.A_Init;

internal interface INewIssueSphereSelection<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewIssueSphereSelection<T> : INewIssueSphereSelection<T> where T : ITrade, new()
{
    private readonly ILiveEventsRepository _liveEventsRepo;
    private readonly ITrade _trade;
    
    private IReadOnlyCollection<string>? _tradeSpecificSphereNamesCache;
    
    public NewIssueSphereSelection(
        ILiveEventsRepository liveEventsRepo,
        IDomainGlossary glossary,
        IStateMediator mediator)
    {
        _liveEventsRepo = liveEventsRepo;
        Glossary = glossary;
        Mediator = mediator;
        
        _trade = new T();
    }
    
    public IDomainGlossary Glossary { get; }
    public IStateMediator Mediator { get; }

    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
        
        List<OutputDto> outputs =
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), _trade.GetSphereOfActionLabel, UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo)),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
        
        if (currentInput.InputType is not TlgInputType.TextMessage ||
            !(await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo))
                .Contains(currentInput.Details.Text.GetValueOrThrow()))
        {
            return WorkflowResponse.CreateWarningChooseReplyKeyboardOptions(
                this, 
                await GetTradeSpecificSphereNamesAsync(_trade, liveEventInfo));
        }

        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput, 
            Mediator.Next(typeof(INewIssueTypeSelection<T>)));
    }

    private async Task<IReadOnlyCollection<string>> GetTradeSpecificSphereNamesAsync(
        ITrade trade, ILiveEventInfo liveEventInfo)
    {
        return _tradeSpecificSphereNamesCache ??= 
            GetAllTradeSpecificSpheres(
                    (await _liveEventsRepo.GetAsync(liveEventInfo))!,
                    trade)
                .Select(static soa => soa.Name)
                .OrderBy(static name => name)
                .ToArray();
    }
}