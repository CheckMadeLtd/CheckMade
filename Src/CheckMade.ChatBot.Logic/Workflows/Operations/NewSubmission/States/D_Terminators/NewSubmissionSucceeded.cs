using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.D_Terminators;

public interface INewSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade, new();

public sealed record NewSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewSubmissionSucceeded<T> where T : ITrade, new();