using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflowState
{
    IDomainGlossary Glossary { get; }
}