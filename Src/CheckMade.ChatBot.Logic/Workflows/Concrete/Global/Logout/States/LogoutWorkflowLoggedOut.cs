using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;

internal interface ILogoutWorkflowLoggedOut : IWorkflowStateTerminator;

internal sealed record LogoutWorkflowLoggedOut(IDomainGlossary Glossary) : ILogoutWorkflowLoggedOut;