using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueFacilitySelection<T> : IWorkflowState where T : ITrade; 

internal sealed record NewIssueFacilitySelection<T>(
        IDomainGlossary Glossary,
        IStateMediator Mediator,
        ILogicUtils LogicUtils,
        ILiveEventsRepository LiveEventsRepo) 
    : INewIssueFacilitySelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var currentSphere = NewIssueWorkflow.GetLastSelectedSphere(
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput),
            NewIssueWorkflow.GetAllTradeSpecificSpheres(
                (await LiveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow()))!,
                new T()));
        
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Choose affected facility:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(IFacility))),
                ControlPromptsSelection = ControlPrompts.Back,
                EditPreviousOutputMessageId = editMessageId
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            return await WorkflowResponse.CreateAsync(
                currentInput, Mediator.Next(typeof(INewIssueEvidenceEntry<T>)),
                true);
        }

        var selectedControlPrompt = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Back => await WorkflowResponse.CreateAsync(
                currentInput, Mediator.Next(typeof(INewIssueTypeSelection<T>)),
                true),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}