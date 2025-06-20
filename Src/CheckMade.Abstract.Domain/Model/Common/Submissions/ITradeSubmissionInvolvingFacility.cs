using CheckMade.Abstract.Domain.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Abstract.Domain.Model.Common.Trades;

namespace CheckMade.Abstract.Domain.Model.Common.Submissions;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}