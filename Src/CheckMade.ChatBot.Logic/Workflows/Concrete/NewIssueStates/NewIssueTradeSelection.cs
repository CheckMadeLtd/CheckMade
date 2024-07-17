using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTradeSelection : IWorkflowState;

internal record NewIssueTradeSelection(
        IDomainGlossary Glossary,
        LiveEvent LiveEvent,
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
                    new OutputDto
                    {
                        Text = Ui("Please answer only using the buttons above.")
                    },
                    Glossary.GetId(GetType()));
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
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary).MyPrompt(),
                        Glossary.GetId(typeof(NewIssueTypeSelection<SaniCleanTrade>))),
                SiteCleanTrade => 
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary).MyPrompt(),
                        Glossary.GetId(typeof(NewIssueTypeSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(selectedTrade)}: '{selectedTrade.GetType()}'")
            };
        }
        
        var lastKnownLocation = await NewIssueWorkflow.LastKnownLocationAsync(currentInput, LogicUtils);

        var sphere = lastKnownLocation.IsSome
            ? NewIssueWorkflow.SphereNearCurrentUser(LiveEvent, lastKnownLocation.GetValueOrThrow(), selectedTrade)
            : Option<ISphereOfAction>.None();
        
        return sphere.Match(
                soa => new WorkflowResponse(
                    new NewIssueSphereConfirmation(selectedTrade, soa).MyPrompt(),
                    Glossary.GetId(typeof(NewIssueSphereConfirmation))),
                () => new WorkflowResponse(
                    new NewIssueSphereSelection(selectedTrade, LiveEvent, Glossary).MyPrompt(),
                    Glossary.GetId(typeof(NewIssueSphereSelection))));
    }
}