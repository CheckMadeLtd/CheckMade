using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Issues;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;