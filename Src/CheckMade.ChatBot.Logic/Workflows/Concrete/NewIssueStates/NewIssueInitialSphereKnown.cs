using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialSphereKnown : IWorkflowState;

internal class NewIssueInitialSphereKnown(
        ITrade trade,
        ISphereOfAction sphere) 
    : INewIssueInitialSphereKnown
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please confirm: are you at {0} '{1}'?",
                    trade.GetSphereOfActionLabel,
                    sphere.Name),
                
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