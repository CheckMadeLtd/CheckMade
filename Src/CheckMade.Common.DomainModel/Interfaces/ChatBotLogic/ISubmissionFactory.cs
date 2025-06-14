using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Submissions;

namespace CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
