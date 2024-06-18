using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

public class LanguageSettingWorkflow : IWorkflow
{
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        throw new NotImplementedException();
    }
}