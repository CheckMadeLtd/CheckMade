namespace CheckMade.Common.DomainModel.Interfaces.Core;

public interface ITradeSubmission<T> : ISubmission where T : ITrade;