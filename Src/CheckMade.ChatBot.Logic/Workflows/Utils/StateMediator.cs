using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal interface IStateMediator
{
    IWorkflowStateNormal Next(Type nextStateType);
    IWorkflowStateTerminator GetTerminator(Type nextStateTerminatorType);
}

internal sealed record StateMediator(IServiceProvider Sp) : IStateMediator
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