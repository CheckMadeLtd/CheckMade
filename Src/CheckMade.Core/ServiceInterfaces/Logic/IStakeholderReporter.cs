using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Bot.DTOs.Output;
using CheckMade.Core.Model.Common.Trades;

namespace CheckMade.Core.ServiceInterfaces.Logic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<Output>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<Input> inputHistory,
        string currentSubmissionTypeName);
}