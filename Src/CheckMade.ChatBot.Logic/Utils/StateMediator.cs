using CheckMade.ChatBot.Logic.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Utils;

internal interface IStateMediator
{
    IWorkflowStateNormal Next(Type nextStateType);
    IWorkflowStateTerminator Terminate(Type nextStateTerminatorType);
}

internal sealed record StateMediator(IServiceProvider Sp) : IStateMediator
{
    public IWorkflowStateNormal Next(Type nextStateType)
    {
        return (IWorkflowStateNormal)Sp.GetRequiredService(nextStateType);
    }

    public IWorkflowStateTerminator Terminate(Type nextStateTerminatorType)
    {
        return (IWorkflowStateTerminator)Sp.GetRequiredService(nextStateTerminatorType);
    }
}