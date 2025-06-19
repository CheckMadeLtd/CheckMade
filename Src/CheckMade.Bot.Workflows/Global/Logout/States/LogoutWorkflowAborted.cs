using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;

namespace CheckMade.Bot.Workflows.Global.Logout.States;

public interface ILogoutWorkflowAborted : IWorkflowStateTerminator;

public sealed record LogoutWorkflowAborted(IDomainGlossary Glossary) : ILogoutWorkflowAborted;