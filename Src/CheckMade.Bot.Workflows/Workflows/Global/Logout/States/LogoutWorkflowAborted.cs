using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.Bot.Workflows.Workflows.Global.Logout.States;

public interface ILogoutWorkflowAborted : IWorkflowStateTerminator;

public sealed record LogoutWorkflowAborted(IDomainGlossary Glossary) : ILogoutWorkflowAborted;