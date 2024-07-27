using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Issues;

public interface ITradeIssueInvolvingFacility
{
    IFacility Facility { get; }
}