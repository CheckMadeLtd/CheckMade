using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

#pragma warning disable CS9113 // Parameter is unread.

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal class LanguageSettingWorkflow(
        ITlgInputRepository inputRepo,
        ITlgClientPortRoleRepository portRoleRepo) 
    : IWorkflow
{
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        throw new NotImplementedException();
    }

    internal Task<States> DetermineCurrentStateAsync()
    {
        throw new NotImplementedException();
    }
    
    [Flags]
    internal enum States
    {
        
    }
}