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
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please select a Trade:"),
                DomainTermSelection = 
                    Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITrade))) 
            }
        };
    }

    public async Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
        {
            return 
                new WorkflowResponse(
                    new OutputDto { Text = Ui("Please answer only using the buttons above.") },
                    GetType(), Glossary);
        }

        var selectedTrade = 
            (ITrade)Activator.CreateInstance(
                currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!)!; 
        
        if (!selectedTrade.DividesLiveEventIntoSpheresOfAction)
        {
            return selectedTrade switch
            {
                SaniCleanTrade => 
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary)),
                SiteCleanTrade => 
                    new WorkflowResponse(
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
        
        return sphere.Match(
                soa => new WorkflowResponse(
                    new NewIssueSphereConfirmation(selectedTrade, soa, LiveEventRepo, Glossary)),
                () => new WorkflowResponse(
                    new NewIssueSphereSelection(selectedTrade, liveEvent, Glossary)));
    }
}