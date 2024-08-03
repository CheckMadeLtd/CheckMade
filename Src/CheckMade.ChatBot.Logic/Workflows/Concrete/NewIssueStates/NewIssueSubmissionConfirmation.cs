using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionConfirmation<T>;

internal sealed record NewIssueSubmissionConfirmation<T> 
    : INewIssueSubmissionConfirmation<T> where T : ITrade
{
}