using CheckMade.Common.Model.ChatBot.Input;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IStateMediator
{
    IWorkflowState Next(Type nextStateType);
    Task<IWorkflowState> PreviousAsync(TlgInput currentInput);
}

internal record StateMediator(IServiceProvider Sp) : IStateMediator
{
    public IWorkflowState Next(Type nextStateType)
    {
        return (IWorkflowState)Sp.GetRequiredService(nextStateType);
    }

    public async Task<IWorkflowState> PreviousAsync(TlgInput currentInput)
    {
        var logicUtils = Sp.GetRequiredService<ILogicUtils>();

        return Next(await logicUtils.GetPreviousStateTypeAsync(
            currentInput,
            ILogicUtils.DistanceFromCurrentWhenNavigatingToPreviousWorkflowState));
    }
}