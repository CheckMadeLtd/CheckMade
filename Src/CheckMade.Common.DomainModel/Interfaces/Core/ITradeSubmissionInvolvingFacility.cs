namespace CheckMade.Common.DomainModel.Interfaces.Core;

internal interface ITradeSubmissionInvolvingFacility<T> : ITradeSubmission<T> where T : ITrade
{
    IFacility Facility { get; }
}