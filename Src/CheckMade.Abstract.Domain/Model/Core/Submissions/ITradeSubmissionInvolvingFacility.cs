using CheckMade.Abstract.Domain.Model.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Abstract.Domain.Model.Core.Trades;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}