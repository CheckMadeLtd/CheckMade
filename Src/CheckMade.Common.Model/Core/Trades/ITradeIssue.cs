namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeIssue<T> : IIssue where T : ITrade;
