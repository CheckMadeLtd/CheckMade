using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTypeSelection<T> : IWorkflowState where T : ITrade; 

internal record NewIssueTypeSelection<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils) 
    : INewIssueTypeSelection<T> where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please select the type of issue:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITradeIssue<T>))),
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var issueTypeName = 
            currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!.Name;
        
        return issueTypeName switch
        {
            nameof(CleanlinessIssue) => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueFacilitySelection<T>(Glossary, LogicUtils),
                        currentInput.Details.TlgMessageId),
            
            nameof(ConsumablesIssue) => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueConsumablesSelection<T>(
                        Glossary,
                        await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput),
                        LogicUtils),
                    currentInput.Details.TlgMessageId),
            
            nameof(TechnicalIssue) or nameof(StaffIssue) => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueEvidenceEntry<T>(
                        Glossary, LogicUtils), currentInput.Details.TlgMessageId),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.DomainTerm)}: '{issueTypeName}'")
        };
    }
}