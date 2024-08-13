using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Issues;

internal interface ITradeIssueInvolvingFacility<T> : IIssue where T : ITrade
{
    IFacility Facility { get; }
}