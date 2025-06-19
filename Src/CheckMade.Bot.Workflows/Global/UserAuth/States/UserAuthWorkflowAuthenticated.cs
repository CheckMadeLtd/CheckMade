using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;

namespace CheckMade.Bot.Workflows.Global.UserAuth.States;

public interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

public sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;