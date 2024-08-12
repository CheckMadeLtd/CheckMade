using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal interface IOneStepWorkflowTerminator : IWorkflowStateTerminator;

internal sealed record OneStepWorkflowTerminator(IDomainGlossary Glossary) 
    : IOneStepWorkflowTerminator; 