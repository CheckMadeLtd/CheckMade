using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Issues;

namespace CheckMade.Common.Interfaces.ChatBotLogic;

public interface IIssueFactory<T>
{
    Task<IIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
