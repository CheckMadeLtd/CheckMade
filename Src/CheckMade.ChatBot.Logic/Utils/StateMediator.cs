using CheckMade.ChatBot.Logic.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Utils;

internal interface IStateMediator
{
    IWorkflowState Next(Type nextStateType);
}

internal sealed record StateMediator(IServiceProvider Sp) : IStateMediator
{
    public IWorkflowState Next(Type nextStateType)
    {
        return (IWorkflowState)Sp.GetRequiredService(nextStateType);
    }
}