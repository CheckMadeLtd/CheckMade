using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;

internal interface INewSubmissionSucceeded<T> : IWorkflowStateTerminator where T : ITrade, new();

internal sealed record NewSubmissionSucceeded<T>(IDomainGlossary Glossary) 
    : INewSubmissionSucceeded<T> where T : ITrade, new();