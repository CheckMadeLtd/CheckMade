using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Utils.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueConsumablesSelection<T> : IWorkflowState where T : ITrade;

internal sealed record NewIssueConsumablesSelection<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator,
        ILiveEventsRepository LiveEventsRepo) 
    : INewIssueConsumablesSelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        
        var currentSphere = 
            GetLastSelectedSphere(interactiveHistory, 
                GetAllTradeSpecificSpheres(
                    (await LiveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow()))!,
                    new T()));
        
        return new List<OutputDto> 
        {
            new()
            {
                Text = Ui("Choose affected consumables:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(ConsumablesItem))
                        .Where(dt => ((SaniCampDetails)currentSphere.Details).AvailableConsumables.Contains(dt))
                        .Select(dt => 
                            dt.IsToggleOn(interactiveHistory) 
                                ? dt with { Toggle = true } 
                                : dt with { Toggle = false })
                        .ToImmutableReadOnlyCollection()),
                ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back,
                EditPreviousOutputMessageId = editMessageId
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
            return await WorkflowResponse.CreateAsync(
                currentInput, this, true);

        var selectedControlPrompt = 
            currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Save =>
                await WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueReview<T>))),
            
            (long)ControlPrompts.Back => 
                await WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueTypeSelection<T>)), 
                    true),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}