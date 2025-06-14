using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    IDomainGlossary Glossary { get; }
}