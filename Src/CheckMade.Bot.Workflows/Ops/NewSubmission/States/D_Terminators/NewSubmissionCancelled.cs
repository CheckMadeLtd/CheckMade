using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.D_Terminators;

public interface INewSubmissionCancelled<T> : IWorkflowStateTerminator where T : ITrade, new();

public sealed record NewSubmissionCancelled<T>(IDomainGlossary Glossary) : INewSubmissionCancelled<T> where T : ITrade, new();
