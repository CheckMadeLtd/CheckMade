using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Interfaces.BusinessLogic;

public interface IStakeholderReporter
{
    Task<IReadOnlyCollection<LogicalPort>> GetNewIssueNotificationRecipientsAsync<T>(
        IReadOnlyCollection<TlgInput> interactiveHistory,
        string currentIssueTypeName) where T : ITrade, new();
}