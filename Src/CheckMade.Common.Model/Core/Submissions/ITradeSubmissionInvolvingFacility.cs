using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Submissions;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}