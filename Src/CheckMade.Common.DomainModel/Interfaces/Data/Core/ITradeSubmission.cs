namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;