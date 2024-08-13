using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;

internal interface INewIssueSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade, new();

internal sealed record NewIssueSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewIssueSubmissionSucceeded<T> where T : ITrade, new();