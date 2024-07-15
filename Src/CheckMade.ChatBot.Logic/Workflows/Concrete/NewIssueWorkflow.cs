using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    
}

internal class NewIssueWorkflow(
        INewIssueInitialTradeUnknown initialTradeUnknown,
        IDomainGlossary glossary) 
    : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        if (currentInput.ResultantWorkflow.IsNone)
        {
            var isCurrentRoleTradeSpecific = 
                currentInput
                    .OriginatorRole.GetValueOrThrow()
                    .RoleType
                    .GetTradeInstance().IsSome;

            if (!isCurrentRoleTradeSpecific)
            {
                return new WorkflowResponse(
                    initialTradeUnknown.MyPrompt(),
                    glossary.GetId(typeof(INewIssueInitialTradeUnknown)));
            }
            
            
        }
        
        return currentInput.ResultantWorkflow.GetValueOrThrow().InStateId switch
        {
            
        }
    }
}