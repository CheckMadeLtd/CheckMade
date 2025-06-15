using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Global.UserAuth.States;

public interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

public sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;