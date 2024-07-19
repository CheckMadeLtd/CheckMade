using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereSelection : IWorkflowState;

internal record NewIssueSphereSelection(
    ITrade Trade,
    ILiveEventInfo LiveEventInfo,
    ILiveEventsRepository LiveEventsRepo,
    IDomainGlossary Glossary) 
    : INewIssueSphereSelection
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = UiConcatenate(
                    Ui("Please select a "),
                    Trade.GetSphereOfActionLabel,
                    UiNoTranslate(":")),
                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    await GetTradeSpecificSphereNamesAsync(Trade))
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        return currentInput switch
        {
            { InputType: not TlgInputType.TextMessage } =>
                WorkflowResponse.CreateOnlyChooseReplyKeyboardOptionResponse(
                    this, await GetTradeSpecificSphereNamesAsync(Trade)),
            
            { Details.Text: var text } 
                when !(await GetTradeSpecificSphereNamesAsync(Trade))
                    .Contains(text.GetValueOrThrow()) => 
                WorkflowResponse.CreateOnlyChooseReplyKeyboardOptionResponse(
                    this, await GetTradeSpecificSphereNamesAsync(Trade)),
                
            _ => Trade switch
            {
                SaniCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary)),

                SiteCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary)),

                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(Trade)}: '{Trade.GetType()}'")
            }
        };
    }

    private async Task<IReadOnlyCollection<string>> GetTradeSpecificSphereNamesAsync(ITrade trade) =>
        (await LiveEventsRepo.GetAsync(LiveEventInfo))!
        .DivIntoSpheres
        .Where(soa => soa.GetTradeType() == trade.GetType())
        .Select(soa => soa.Name)
        .ToImmutableReadOnlyCollection();
}