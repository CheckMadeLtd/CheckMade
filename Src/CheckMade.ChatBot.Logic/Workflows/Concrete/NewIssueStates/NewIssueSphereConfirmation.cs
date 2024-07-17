using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmation : IWorkflowState;

internal record NewIssueSphereConfirmation(
        ITrade Trade,
        ISphereOfAction Sphere,
        ILiveEventsRepository LiveEventRepo,
        IDomainGlossary Glossary) 
    : INewIssueSphereConfirmation
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please confirm: are you at '{0}'?", Sphere.Name),
                ControlPromptsSelection = ControlPrompts.YesNo
            }
        };
    }

    public async Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
        {
            return 
                new WorkflowResponse(
                    new OutputDto
                    {
                        Text = Ui("Please answer only using the buttons above.")
                    },
                    Glossary.GetId(GetType()));
        }

        var liveEventContext = currentInput.LiveEventContext.GetValueOrThrow();
        
        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (int)ControlPrompts.Yes => Trade switch
            {
                SaniCleanTrade => 
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary).MyPrompt(),
                        Glossary.GetId(typeof(NewIssueTypeSelection<SaniCleanTrade>))),
                SiteCleanTrade =>
                    new WorkflowResponse(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary).MyPrompt(),
                        Glossary.GetId(typeof(NewIssueTypeSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException(
                    $"Unhandled type of {nameof(Trade)}: '{Trade.GetType()}'")
            },
            
            (int)ControlPrompts.No => 
                new WorkflowResponse(
                    new NewIssueSphereSelection(
                        Trade,
                        (await LiveEventRepo.GetAsync(liveEventContext))!, 
                        Glossary).MyPrompt(),
                    Glossary.GetId(typeof(NewIssueSphereSelection))),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}