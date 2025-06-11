using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.D_Terminators;

internal interface INewIssueCancelled<T> : IWorkflowStateTerminator where T : ITrade, new();

internal sealed record NewIssueCancelled<T>(IDomainGlossary Glossary) : INewIssueCancelled<T> where T : ITrade, new();
