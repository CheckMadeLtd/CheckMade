using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.A_Init;

internal interface INewSubmissionTradeSelection : IWorkflowStateNormal;

internal sealed record NewSubmissionTradeSelection(
    IDomainGlossary Glossary,
    ILiveEventsRepository LiveEventRepo,
    IGeneralWorkflowUtils WorkflowUtils,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IStateMediator Mediator)
    : INewSubmissionTradeSelection
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
        if (currentInput.InputType is not InputType.CallbackQuery)
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