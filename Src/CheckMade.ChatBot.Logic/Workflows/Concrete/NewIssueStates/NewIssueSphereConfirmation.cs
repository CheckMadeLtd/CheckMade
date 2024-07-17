using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmation : IWorkflowState;

internal record NewIssueSphereConfirmation(
        ITrade Trade,
        ISphereOfAction Sphere) 
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

    public Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}