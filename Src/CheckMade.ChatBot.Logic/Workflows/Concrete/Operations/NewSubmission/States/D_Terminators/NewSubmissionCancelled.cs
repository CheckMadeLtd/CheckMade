using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;

internal interface INewSubmissionCancelled<T> : IWorkflowStateTerminator where T : ITrade, new();

internal sealed record NewSubmissionCancelled<T>(IDomainGlossary Glossary) : INewSubmissionCancelled<T> where T : ITrade, new();
