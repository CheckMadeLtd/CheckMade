using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;

namespace CheckMade.Abstract.Domain.Interfaces.Bot.Logic;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<Input> inputs);
} 
