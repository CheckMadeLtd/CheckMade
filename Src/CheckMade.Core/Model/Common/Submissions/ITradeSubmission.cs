using CheckMade.Core.Model.Common.Trades;

namespace CheckMade.Core.Model.Common.Submissions;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;