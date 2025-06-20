using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.Submissions;

namespace CheckMade.Core.ServiceInterfaces.Bot;

public interface ISubmissionFactory<T>
{
    Task<ISubmission> CreateAsync(IReadOnlyCollection<Input> inputs);
} 
