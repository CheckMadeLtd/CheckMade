using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface ILanguageSettingWorkflow : IWorkflow
{
    Task<LanguageSettingWorkflow.States> DetermineCurrentStateAsync();
}

internal class LanguageSettingWorkflow(
        ITlgClientPortRoleRepository portRoleRepo) 
    : ILanguageSettingWorkflow
{
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        throw new NotImplementedException();
    }

    public Task<States> DetermineCurrentStateAsync()
    {
        throw new NotImplementedException();
    }
    
    [Flags]
    internal enum States
    {
        Initial = 1,
        ReceivedLanguageSetting = 1<<1
    }
}