using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.NewIssueUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.A_Init;

internal interface INewIssueTradeSelection : IWorkflowStateNormal;

internal sealed record NewIssueTradeSelection(
    IDomainGlossary Glossary,
    ILiveEventsRepository LiveEventRepo,
    IGeneralWorkflowUtils WorkflowUtils,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IStateMediator Mediator)
    : INewIssueTradeSelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("Please select a Trade:"),
                DomainTermSelection = 
                    Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITrade))),
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var selectedTradeDt = currentInput.Details.DomainTerm.GetValueOrThrow(); 
        var selectedTrade = (ITrade)Activator.CreateInstance(selectedTradeDt.TypeValue!)!; 
        
        var promptTransition =
            new PromptTransition(
                new OutputDto
                {
                    Text = UiConcatenate(
                        UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                        UiNoTranslate(" "),
                        Glossary.GetUi(selectedTradeDt)),
                    UpdateExistingOutputMessageId = currentInput.TlgMessageId
                });
        
        return await (await GetSphereNearUserAsync()).Match(
            _ => selectedTrade switch 
            { 
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereConfirmation<SanitaryTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereConfirmation<SiteCleanTrade>)),
                    promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            }, 
            () => selectedTrade switch
            {
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereSelection<SanitaryTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>)),
                    promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            });

        async Task<Option<ISphereOfAction>> GetSphereNearUserAsync()
        {
            var lastKnownLocation = 
                await LastKnownLocationAsync(currentInput, WorkflowUtils);

            return lastKnownLocation.IsSome
                ? await SphereNearCurrentUserAsync(
                    currentInput.LiveEventContext.GetValueOrThrow(),
                    LiveEventRepo,
                    lastKnownLocation.GetValueOrThrow(), selectedTrade,
                    currentInput, RoleBindingsRepo)
                : Option<ISphereOfAction>.None();
        }
    }
}