using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueFacilitySelection : IWorkflowState; 

internal record NewIssueFacilitySelection<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils) 
    : INewIssueFacilitySelection where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
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
                    EditReplyMarkupOfMessageId = editMessageId
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
                        new NewIssueConsumablesSelection(
                            Glossary,
                            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput),
                            (ITrade)Activator.CreateInstance(typeof(T))!,
                            LogicUtils),
                        currentInput.Details.TlgMessageId),

                _ => await WorkflowResponse.CreateAsync(
                    new NewIssueEvidenceEntry(Glossary, LogicUtils),
                    currentInput.Details.TlgMessageId)
            };
        }

        var selectedControlPrompt = currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Back => await WorkflowResponse.CreateAsync(
                new NewIssueTypeSelection<T>(Glossary, LogicUtils),
                currentInput.Details.TlgMessageId),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}