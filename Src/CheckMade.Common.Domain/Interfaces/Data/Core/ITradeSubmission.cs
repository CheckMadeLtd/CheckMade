namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;