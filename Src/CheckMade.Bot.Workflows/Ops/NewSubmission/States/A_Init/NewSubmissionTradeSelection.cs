using System.Collections.Immutable;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;

public interface INewSubmissionTradeSelection : IWorkflowStateNormal;

public sealed record NewSubmissionTradeSelection(
    IDomainGlossary Glossary,
    ILiveEventsRepository LiveEventRepo,
    IGeneralWorkflowUtils WorkflowUtils,
    IAgentRoleBindingsRepository RoleBindingsRepo,
    IStateMediator Mediator)
    : INewSubmissionTradeSelection
{
    public Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        List<Output> outputs =
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
        
        return Task.FromResult<IReadOnlyCollection<Output>>(
            previousPromptFinalizer.Match(
                ppf => outputs.Prepend(ppf).ToImmutableArray(),
                () => outputs.ToImmutableArray()));
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType is not InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var selectedTradeDt = currentInput.Details.DomainTerm.GetValueOrThrow(); 
        var selectedTrade = (ITrade)Activator.CreateInstance(selectedTradeDt.TypeValue!)!; 
        
        var promptTransition =
            new PromptTransition(
                new Output
                {
                    Text = UiConcatenate(
                        UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                        UiNoTranslate(" "),
                        Glossary.GetUi(selectedTradeDt)),
                    UpdateExistingOutputMessageId = currentInput.MessageId
                });
        
        return await (await GetSphereNearUserAsync()).Match(
            _ => selectedTrade switch 
            { 
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereConfirmation<SanitaryTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereConfirmation<SiteCleanTrade>)),
                    promptTransition),
                
                _ => throw new InvalidOperationException($"Unhandled {nameof(selectedTrade)}: " +
                                                         $"'{selectedTrade.GetType()}'")
            }, 
            () => selectedTrade switch
            {
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereSelection<SanitaryTrade>)),
                    promptTransition),
                
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereSelection<SiteCleanTrade>)),
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