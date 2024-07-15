using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialSphereUnknown : IWorkflowState;

internal class NewIssueInitialSphereUnknown(
        ITrade trade,
        LiveEvent liveEvent) 
    : INewIssueInitialSphereUnknown
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please select a {0}:",
                    trade.GetSphereOfActionLabel),

                PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(
                    liveEvent.DivIntoSpheres
                        .Select(soa => soa.Name)
                        .ToImmutableReadOnlyCollection())
            }
        };
    }

    public Task<Result<WorkflowResponse>> ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync()
    {
        throw new NotImplementedException();
    }
}