namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;