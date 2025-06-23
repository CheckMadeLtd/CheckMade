using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using CheckMade.Core.Model.Common.Trades;

namespace CheckMade.Core.ServiceInterfaces.Logic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<Output>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<Input> inputHistory,
        string currentSubmissionTypeName);
}