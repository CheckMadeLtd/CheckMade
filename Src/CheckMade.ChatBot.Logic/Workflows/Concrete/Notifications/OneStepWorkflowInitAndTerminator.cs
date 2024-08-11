using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal interface IOneStepWorkflowInitAndTerminator : IWorkflowStateInit, IWorkflowStateTerminator;

internal sealed record OneStepWorkflowInitAndTerminator(IDomainGlossary Glossary) 
    : IOneStepWorkflowInitAndTerminator; 