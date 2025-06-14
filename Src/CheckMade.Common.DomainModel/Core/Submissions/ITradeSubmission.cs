using CheckMade.Common.DomainModel.Core.Trades;

namespace CheckMade.Common.DomainModel.Core.Submissions;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;