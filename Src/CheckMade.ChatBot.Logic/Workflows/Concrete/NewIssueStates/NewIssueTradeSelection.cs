using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTradeSelection : IWorkflowState;

internal sealed record NewIssueTradeSelection(
        IDomainGlossary Glossary,
        ILiveEventsRepository LiveEventRepo,
        ILogicUtils LogicUtils,
        IStateMediator Mediator)
    : INewIssueTradeSelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please select a Trade:"),
                    DomainTermSelection = 
                        Option<IReadOnlyCollection<DomainTerm>>.Some(Glossary.GetAll(typeof(ITrade))),
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var selectedTrade = 
            (ITrade)Activator.CreateInstance(
                currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!)!; 
        
        var lastKnownLocation = 
            await NewIssueWorkflow.LastKnownLocationAsync(currentInput, LogicUtils);

        var liveEvent =
            (await LiveEventRepo.GetAsync(
                currentInput.LiveEventContext.GetValueOrThrow()))!;
        
        var sphere = lastKnownLocation.IsSome
            ? NewIssueWorkflow.SphereNearCurrentUser(
                liveEvent, lastKnownLocation.GetValueOrThrow(), selectedTrade)
            : Option<ISphereOfAction>.None();
        
        return await sphere.Match(
            _ => selectedTrade switch 
            { 
                SaniCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SaniCleanTrade>))),
                
                SiteCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SiteCleanTrade>))),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            }, 
            () => selectedTrade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SaniCleanTrade>))),
                
                SiteCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>))),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            });
    }
}