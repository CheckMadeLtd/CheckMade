using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Common.Submissions;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Bot;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<Input> inputs);
} 
