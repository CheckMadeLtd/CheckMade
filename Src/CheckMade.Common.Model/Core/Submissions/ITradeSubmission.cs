using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Submissions;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;