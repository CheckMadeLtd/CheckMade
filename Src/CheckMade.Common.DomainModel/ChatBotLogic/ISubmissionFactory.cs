using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Submissions;

namespace CheckMade.Common.DomainModel.ChatBotLogic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
