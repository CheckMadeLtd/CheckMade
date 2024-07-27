using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Issues;

internal interface IIssueInvolvingFacility
{
    IFacility Facility { get; }
}