using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.D_Terminators;

public interface INewSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade, new();

public sealed record NewSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewSubmissionSucceeded<T> where T : ITrade, new();