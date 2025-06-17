using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Trades;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using static CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.A_Init;

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