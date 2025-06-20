using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Global.Logout.States;

public interface ILogoutWorkflowAborted : IWorkflowStateTerminator;

public sealed record LogoutWorkflowAborted(IDomainGlossary Glossary) : ILogoutWorkflowAborted;