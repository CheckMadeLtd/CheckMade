using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels;

namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeIssueWithEvidence
{
    IssueEvidence Evidence { get; }
}