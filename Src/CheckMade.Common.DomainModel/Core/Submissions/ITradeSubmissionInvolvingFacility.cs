using CheckMade.Common.DomainModel.Core.LiveEvents;
using CheckMade.Common.DomainModel.Core.Trades;

namespace CheckMade.Common.DomainModel.Core.Submissions;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}