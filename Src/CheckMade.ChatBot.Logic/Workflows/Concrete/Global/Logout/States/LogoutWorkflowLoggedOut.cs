using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;

internal interface ILogoutWorkflowLoggedOut : IWorkflowStateTerminator;

internal sealed record LogoutWorkflowLoggedOut(IDomainGlossary Glossary) : ILogoutWorkflowLoggedOut;