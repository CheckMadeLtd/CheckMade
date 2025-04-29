using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.NewIssueUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;

internal interface INewIssueFacilitySelection<T> : IWorkflowStateNormal where T : ITrade, new(); 

internal sealed record NewIssueFacilitySelection<T>(
    IDomainGlossary Glossary,
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    ILiveEventsRepository LiveEventsRepo) 
    : INewIssueFacilitySelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var currentSphere = 
            GetLastSelectedSphere<T>(
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput), 
                GetAllTradeSpecificSpheres(
                    (await LiveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow()))!,
                    new T()));
        
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("Choose affected facility:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary
                        .GetAll(typeof(IFacility))
                        .Where(dt => currentSphere.Details.AvailableFacilities.Contains(dt))
                        .ToImmutableArray()),
                ControlPromptsSelection = ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<ResultOld<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var selectedFacility = currentInput.Details.DomainTerm.GetValueOrThrow();
            
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, 
                Mediator.Next(typeof(INewIssueEvidenceEntry<T>)),
                new PromptTransition(
                    new OutputDto
                    {
                        Text = UiConcatenate(
                            UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                            UiNoTranslate(" "),
                            Glossary.GetUi(selectedFacility)),
                        UpdateExistingOutputMessageId = currentInput.TlgMessageId
                    }));
        }

        return // on ControlPrompts.Back
            await WorkflowResponse.CreateFromNextStateAsync(
                currentInput,
                Mediator.Next(typeof(INewIssueTypeSelection<T>)),
                new PromptTransition(true));
    }
}