using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Utils.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmation<T> : IWorkflowState where T : ITrade;

internal sealed record NewIssueSphereConfirmation<T>(
        ILiveEventsRepository LiveEventsRepo,    
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : INewIssueSphereConfirmation<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var liveEvent = (await LiveEventsRepo.GetAsync(
            currentInput.LiveEventContext.GetValueOrThrow()))!;
        
        var lastKnownLocation = 
            await LastKnownLocationAsync(currentInput, GeneralWorkflowUtils);

        var sphere = lastKnownLocation.IsSome
            ? SphereNearCurrentUser(
                liveEvent, lastKnownLocation.GetValueOrThrow(), 
                new T())
            : Option<ISphereOfAction>.None();

        if (sphere.IsNone)
        {
            // ToDo: break? Handle case where user has moved away since confirming Sphere in last step! 
            // It's an edge case. I think should lead back to SphereUnknown state.
            // Or maybe pass Option<ISphereOfAction> to the constructor and handle it there? 
        }
        
        return 
            new List<OutputDto> 
            {
                new()
                {
                    Text = Ui("Please confirm: are you at '{0}'?", sphere.GetValueOrThrow().Name),
                    ControlPromptsSelection = ControlPrompts.YesNo,
                    EditPreviousOutputMessageId = editMessageId
                }
            };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);
        
        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (int)ControlPrompts.Yes => 
                await WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueTypeSelection<T>))),
            
            (int)ControlPrompts.No => 
                await WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<T>))),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}