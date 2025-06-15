using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.Logout.States;

internal interface ILogoutWorkflowAborted : IWorkflowStateTerminator;

internal sealed record LogoutWorkflowAborted(IDomainGlossary Glossary) : ILogoutWorkflowAborted;