using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialSphereUnknown : IWorkflowState;

internal record NewIssueInitialSphereUnknown : INewIssueInitialSphereUnknown
{
    private readonly ITrade _trade;
    private readonly IDomainGlossary _glossary;
    private readonly IReadOnlyCollection<string> _tradeSpecificSphereNames;
    
    public NewIssueInitialSphereUnknown(ITrade trade, LiveEvent liveEvent, IDomainGlossary glossary)
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
                Text = Ui("Please select a {0}:",
                    _trade.GetSphereOfActionLabel),

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
                            Text = Ui("Expected a simple text with the name of a {0}! Please try again.",
                                _trade.GetSphereOfActionLabel)
                        },
                        _glossary.GetId(GetType()))),
            
            { Details.Text: var text } 
                when !_tradeSpecificSphereNames.Contains(text.GetValueOrThrow()) => 
                Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new OutputDto
                        {
                            Text = Ui("This is not a valid {0} name. Please choose from the valid options.",
                                _trade.GetSphereOfActionLabel)
                        }, 
                        _glossary.GetId(GetType()))),
            
            _ => Task.FromResult<Result<WorkflowResponse>>(
                    new WorkflowResponse(
                        new NewIssueSphereConfirmed().MyPrompt(),
                        _glossary.GetId(typeof(NewIssueSphereConfirmed))))
        };
    }
}