using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionSucceeded<T>;

internal sealed record NewIssueSubmissionSucceeded<T> 
    : INewIssueSubmissionSucceeded<T> where T : ITrade
{
}