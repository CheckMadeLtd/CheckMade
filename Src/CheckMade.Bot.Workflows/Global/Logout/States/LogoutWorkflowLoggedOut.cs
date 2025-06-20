using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Global.Logout.States;

public interface ILogoutWorkflowLoggedOut : IWorkflowStateTerminator;

public sealed record LogoutWorkflowLoggedOut(IDomainGlossary Glossary) : ILogoutWorkflowLoggedOut;