using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTradeSelection : IWorkflowState;

internal record NewIssueTradeSelection(
        IDomainGlossary Glossary,
        ILiveEventsRepository LiveEventRepo,
        ILogicUtils LogicUtils) 
    : INewIssueTradeSelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please select a Trade:"),
                    DomainTermSelection = 
                        Option<IReadOnlyCollection<DomainTerm>>.Some(
                            Glossary.GetAll(typeof(ITrade))) 
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateOnlyUseInlineKeyboardButtonResponse(this);

        var selectedTrade = 
            (ITrade)Activator.CreateInstance(
                currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!)!; 
        
        if (!selectedTrade.DividesLiveEventIntoSpheresOfAction)
        {
            return selectedTrade switch
            {
                SaniCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary)),
                SiteCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary)),
                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(selectedTrade)}: '{selectedTrade.GetType()}'")
            };
        }
        
        var lastKnownLocation = await NewIssueWorkflow.LastKnownLocationAsync(currentInput, LogicUtils);

        var liveEvent =
            (await LiveEventRepo.GetAsync(
                currentInput.LiveEventContext.GetValueOrThrow()))!;
        
        var sphere = lastKnownLocation.IsSome
            ? NewIssueWorkflow.SphereNearCurrentUser(liveEvent, lastKnownLocation.GetValueOrThrow(), selectedTrade)
            : Option<ISphereOfAction>.None();
        
        return await sphere.Match(
            soa => WorkflowResponse.CreateAsync(
                    new NewIssueSphereConfirmation(selectedTrade, soa, LiveEventRepo, Glossary)),
                () => WorkflowResponse.CreateAsync(
                    new NewIssueSphereSelection(selectedTrade, liveEvent, LiveEventRepo, Glossary)));
    }
}