using CheckMade.Abstract.Domain.Model.Common.Trades;

namespace CheckMade.Abstract.Domain.Model.Common.Submissions;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;