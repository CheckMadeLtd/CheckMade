namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}