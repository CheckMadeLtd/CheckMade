using CheckMade.Abstract.Domain.Model.Core.Trades;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;