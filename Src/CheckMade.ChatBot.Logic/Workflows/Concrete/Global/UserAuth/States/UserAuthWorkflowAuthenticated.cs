using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;

internal interface IUserAuthWorkflowAuthenticated : IWorkflowStateTerminator;

internal sealed record UserAuthWorkflowAuthenticated(IDomainGlossary Glossary) : IUserAuthWorkflowAuthenticated;