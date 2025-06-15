using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Logic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<OutputDto>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<TlgInput> inputHistory,
        string currentSubmissionTypeName);
}