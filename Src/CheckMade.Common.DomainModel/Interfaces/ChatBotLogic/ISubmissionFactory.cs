using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.Core.Submissions;

namespace CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
