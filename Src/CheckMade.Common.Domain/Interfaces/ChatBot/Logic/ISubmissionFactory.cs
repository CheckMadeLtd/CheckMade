using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<Input> inputs);
} 
