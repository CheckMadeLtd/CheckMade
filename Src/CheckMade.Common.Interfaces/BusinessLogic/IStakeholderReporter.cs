using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Interfaces.BusinessLogic;

public interface IStakeholderReporter<T> where T : ITrade, new()
{
    Task<IReadOnlyCollection<OutputDto>> GetNewIssueNotificationsAsync(
        IReadOnlyCollection<TlgInput> inputHistory,
        string currentIssueTypeName);
}