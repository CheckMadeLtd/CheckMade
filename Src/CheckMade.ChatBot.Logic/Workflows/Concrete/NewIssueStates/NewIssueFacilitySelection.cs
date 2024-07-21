using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueFacilitySelection<T> : IWorkflowState where T : ITrade; 

internal record NewIssueFacilitySelection<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils) 
    : INewIssueFacilitySelection<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Choose affected facility:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITradeFacility<T>))),
                    ControlPromptsSelection = ControlPrompts.Back,
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
        {
            var selectedFacilityName =
                currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!.Name;

            return selectedFacilityName switch
            {
                nameof(Consumables) =>
                    await WorkflowResponse.CreateAsync(
                        currentInput,
                        new NewIssueConsumablesSelection<T>(
                            Glossary,
                            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput),
                            LogicUtils),
                        true),

                _ => await WorkflowResponse.CreateAsync(
                    currentInput,
                    new NewIssueEvidenceEntry<T>(Glossary, LogicUtils),
                    true)
            };
        }

        var selectedControlPrompt = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Back => await WorkflowResponse.CreateAsync(
                currentInput,
                new NewIssueTypeSelection<T>(Glossary, LogicUtils),
                true),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}