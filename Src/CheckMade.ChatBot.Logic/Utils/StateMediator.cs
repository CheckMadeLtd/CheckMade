using CheckMade.ChatBot.Logic.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Utils;

internal interface IStateMediator
{
    IWorkflowStateActive Next(Type nextStateType);
    IWorkflowStateTerminator Terminate(Type nextStateTerminatorType);
}

internal sealed record StateMediator(IServiceProvider Sp) : IStateMediator
{
    public IWorkflowStateActive Next(Type nextStateType)
    {
        return (IWorkflowStateActive)Sp.GetRequiredService(nextStateType);
    }

    public IWorkflowStateTerminator Terminate(Type nextStateTerminatorType)
    {
        return (IWorkflowStateTerminator)Sp.GetRequiredService(nextStateTerminatorType);
    }
}