using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Bot.Workflows.Utils;

public interface IStateMediator
{
    IWorkflowStateNormal Next(Type nextStateType);
    IWorkflowStateTerminator GetTerminator(Type nextStateTerminatorType);
}

public sealed record StateMediator(IServiceProvider Sp) : IStateMediator
{
    public IWorkflowStateNormal Next(Type nextStateType)
    {
        return (IWorkflowStateNormal)Sp.GetRequiredService(nextStateType);
    }

    public IWorkflowStateTerminator GetTerminator(Type nextStateTerminatorType)
    {
        return (IWorkflowStateTerminator)Sp.GetRequiredService(nextStateTerminatorType);
    }
}