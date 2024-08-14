using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Submissions.Issues;

public interface ITradeIssue<T> : IIssue where T : ITrade;