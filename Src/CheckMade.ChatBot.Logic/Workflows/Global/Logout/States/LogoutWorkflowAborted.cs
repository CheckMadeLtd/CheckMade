using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.Logout.States;

public interface ILogoutWorkflowAborted : IWorkflowStateTerminator;

public sealed record LogoutWorkflowAborted(IDomainGlossary Glossary) : ILogoutWorkflowAborted;