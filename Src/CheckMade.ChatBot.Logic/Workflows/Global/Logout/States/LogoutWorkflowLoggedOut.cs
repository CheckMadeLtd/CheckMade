using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.Logout.States;

internal interface ILogoutWorkflowLoggedOut : IWorkflowStateTerminator;

internal sealed record LogoutWorkflowLoggedOut(IDomainGlossary Glossary) : ILogoutWorkflowLoggedOut;