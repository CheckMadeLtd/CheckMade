using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;

internal interface ILogoutWorkflowConfirm : IWorkflowStateNormal;

internal sealed record LogoutWorkflowConfirm(
        IDomainGlossary Glossary, 
        IStateMediator Mediator) 
    : ILogoutWorkflowConfirm
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<int> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}