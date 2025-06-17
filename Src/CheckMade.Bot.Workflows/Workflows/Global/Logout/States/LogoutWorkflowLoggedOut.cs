using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.Bot.Workflows.Workflows.Global.Logout.States;

public interface ILogoutWorkflowLoggedOut : IWorkflowStateTerminator;

public sealed record LogoutWorkflowLoggedOut(IDomainGlossary Glossary) : ILogoutWorkflowLoggedOut;