using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.Bot.Workflows.Workflows.Global.UserAuth.States;

public interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

public sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;