using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Common.Trades;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Logic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<Output>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<Input> inputHistory,
        string currentSubmissionTypeName);
}