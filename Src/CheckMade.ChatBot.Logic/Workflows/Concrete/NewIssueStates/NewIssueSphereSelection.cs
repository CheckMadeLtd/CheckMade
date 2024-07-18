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
                    await GetTradeSpecificSphereNames(Trade))
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        return currentInput switch
        {
            { InputType: not TlgInputType.TextMessage } =>
                    new WorkflowResponse(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                Ui("Expected a simple text! Please try again entering a "),
                                Trade.GetSphereOfActionLabel,
                                UiNoTranslate(":"))
                        },
                        GetType(), Glossary),

            { Details.Text: var text }
                when !(await GetTradeSpecificSphereNames(Trade))
                    .Contains(text.GetValueOrThrow()) => 
                new WorkflowResponse(
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            Ui("This is not a valid name. Please choose from the valid options for a "),
                            Trade.GetSphereOfActionLabel,
                            UiNoTranslate(":"))
                    },
                    GetType(), Glossary),

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

    private async Task<IReadOnlyCollection<string>> GetTradeSpecificSphereNames(ITrade trade) =>
        (await LiveEventsRepo.GetAsync(LiveEventInfo))!
        .DivIntoSpheres
        .Where(soa => soa.GetTradeType() == trade.GetType())
        .Select(soa => soa.Name)
        .ToImmutableReadOnlyCollection();
}