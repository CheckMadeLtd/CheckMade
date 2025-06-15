using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;

internal interface INewSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade, new();

internal sealed record NewSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewSubmissionSucceeded<T> where T : ITrade, new();