using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    IDomainGlossary Glossary { get; }
}