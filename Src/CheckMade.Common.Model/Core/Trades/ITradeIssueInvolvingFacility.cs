using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeIssueInvolvingFacility
{
    ITradeFacility Facility { get; }
}