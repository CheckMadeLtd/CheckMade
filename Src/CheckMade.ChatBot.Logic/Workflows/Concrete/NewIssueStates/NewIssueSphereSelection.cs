using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereSelection : IWorkflowState;

internal record NewIssueSphereSelection : INewIssueSphereSelection
{
    private readonly ITrade _trade;
    private readonly IDomainGlossary _glossary;
    private readonly IReadOnlyCollection<string> _tradeSpecificSphereNames;
    
    public NewIssueSphereSelection(ITrade trade, LiveEvent liveEvent, IDomainGlossary glossary)
    {
        _trade = trade;
        _glossary = glossary;

        _tradeSpecificSphereNames = liveEvent.DivIntoSpheres
            .Where(soa => soa.GetTradeType() == _trade.GetType())
            .Select(soa => soa.Name)
            .ToImmutableReadOnlyCollection();
    }
    
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "), 
                    _trade.GetSphereOfActionLabel, 
                    UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    _tradeSpecificSphereNames)
            }
        };
    }

    public Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        return currentInput switch
        {
            { InputType: not TlgInputType.TextMessage } => 
                Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                Ui("Expected a simple text! Please try again entering a "),
                                _trade.GetSphereOfActionLabel,
                                UiNoTranslate(":"))
                        },
                        _glossary.GetId(GetType()))),
            
            { Details.Text: var text } 
                when !_tradeSpecificSphereNames.Contains(text.GetValueOrThrow()) => 
                Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                Ui("This is not a valid name. Please choose from the valid options for a "),
                                _trade.GetSphereOfActionLabel,
                                UiNoTranslate(":"))
                        }, 
                        _glossary.GetId(GetType()))),
            
            _ => _trade switch 
            { 
                SaniCleanTrade => Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SaniCleanTrade>(_glossary).MyPrompt(),
                        _glossary.GetId(typeof(NewIssueTypeSelection<SaniCleanTrade>)))),
                
                SiteCleanTrade => Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SiteCleanTrade>(_glossary).MyPrompt(),
                        _glossary.GetId(typeof(NewIssueTypeSelection<SiteCleanTrade>)))),
                
                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(_trade)}: '{_trade.GetType()}'") 
            }
        };
    }
}