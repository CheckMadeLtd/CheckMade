using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.D_Terminators;

public interface INewSubmissionCancelled<T> : IWorkflowStateTerminator where T : ITrade, new();

public sealed record NewSubmissionCancelled<T>(IDomainGlossary Glossary) : INewSubmissionCancelled<T> where T : ITrade, new();
