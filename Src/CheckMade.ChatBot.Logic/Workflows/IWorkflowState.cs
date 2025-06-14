using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    IDomainGlossary Glossary { get; }
}