using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.DomainModel.Interfaces.BusinessLogic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<OutputDto>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<TlgInput> inputHistory,
        string currentSubmissionTypeName);
}