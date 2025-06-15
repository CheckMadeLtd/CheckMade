using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.Output;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Logic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<OutputDto>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<TlgInput> inputHistory,
        string currentSubmissionTypeName);
}