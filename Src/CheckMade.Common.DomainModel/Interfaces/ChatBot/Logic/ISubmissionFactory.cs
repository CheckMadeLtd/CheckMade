using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 
