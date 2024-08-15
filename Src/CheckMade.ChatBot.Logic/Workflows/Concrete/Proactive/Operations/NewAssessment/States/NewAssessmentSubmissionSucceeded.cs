using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewAssessment.States;

internal interface INewAssessmentSubmissionSucceeded : IWorkflowStateTerminator;

internal sealed record NewAssessmentSubmissionSucceeded(IDomainGlossary Glossary) 
    : INewAssessmentSubmissionSucceeded;