using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Bot.Workflows.Global.UserAuth.States;

public interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

public sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;