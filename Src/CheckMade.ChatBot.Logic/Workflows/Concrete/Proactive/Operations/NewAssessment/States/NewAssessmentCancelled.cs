using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewAssessment.States;

internal interface INewAssessmentCancelled : IWorkflowStateTerminator;

internal sealed record NewAssessmentCancelled(IDomainGlossary Glossary) : INewAssessmentCancelled;
