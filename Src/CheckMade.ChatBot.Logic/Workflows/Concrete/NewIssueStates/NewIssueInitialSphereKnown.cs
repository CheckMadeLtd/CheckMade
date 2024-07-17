using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialSphereKnown : IWorkflowState;

internal record NewIssueInitialSphereKnown(
        ITrade Trade,
        ISphereOfAction Sphere) 
    : INewIssueInitialSphereKnown
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please confirm: are you at {0} '{1}'?",
                    Trade.GetSphereOfActionLabel,
                    Sphere.Name),
                
                ControlPromptsSelection = ControlPrompts.YesNo
            }
        };
    }

    public Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}