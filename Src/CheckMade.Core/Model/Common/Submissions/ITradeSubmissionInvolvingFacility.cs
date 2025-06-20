using CheckMade.Core.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Core.Model.Common.Trades;

namespace CheckMade.Core.Model.Common.Submissions;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}