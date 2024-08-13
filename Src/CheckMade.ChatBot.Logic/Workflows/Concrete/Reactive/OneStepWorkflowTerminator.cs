using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive;

internal interface IOneStepWorkflowTerminator : IWorkflowStateTerminator;

internal sealed record OneStepWorkflowTerminator(IDomainGlossary Glossary) 
    : IOneStepWorkflowTerminator; 