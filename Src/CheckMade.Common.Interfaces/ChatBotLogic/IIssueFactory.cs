using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Issues;

namespace CheckMade.Common.Interfaces.ChatBotLogic;

public interface IIssueFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
