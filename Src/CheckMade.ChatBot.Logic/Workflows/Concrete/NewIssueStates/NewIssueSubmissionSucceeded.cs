using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade;

internal sealed record NewIssueSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewIssueSubmissionSucceeded<T> where T : ITrade;